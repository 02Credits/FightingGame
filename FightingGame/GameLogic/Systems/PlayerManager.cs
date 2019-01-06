using FightingGame.GameLogic.Systems.Interfaces;
using System;
using System.Linq;

namespace FightingGame.GameLogic.Systems
{
    public enum PlayerState
    {
        Falling,
        Idle,
        WalkingLeft,
        WalkingRight
    }

    public class Player
    {
        public string PlayerID { get; }
        public PlayerState CurrentState { get; }
        public float XVelocity { get; }
        public float YVelocity { get; }

        public Player(string playerID, PlayerState state)
        {
            PlayerID = playerID;
            CurrentState = state;
        }

        public Player SetState(PlayerState state) => new Player(PlayerID, state);
    }

    public class Velocity
    {
        public float X{ get; }
        public float Y{ get; }

        public Velocity(float x, float y)
        {
            X = x;
            Y = y;
        }

        public Velocity SetXVelocity(float x) => new Velocity(x, Y);
        public Velocity SetYVelocity(float y) => new Velocity(X, y);

        public Velocity UpdateXVelocity(Func<float, float> update) => SetXVelocity(update(X));
        public Velocity UpdateYVelocity(Func<float, float> update) => SetYVelocity(update(Y));

        public static Position operator +(Position position, Velocity velocity)
            => new Position((int)(position.X + velocity.X), (int)(position.Y + velocity.Y));
        public static Position operator +(Velocity velocity, Position position) => position + velocity;
    }

    public class PlayerManager : SystemBase, IInitializableSystem, IEntityUpdaterSystem, IUpdatableSystem
    {
        public const int GroundLevel = -50;
        public const float GroundFriction = 0.9f;
        public const float AirFriction = 0.999f;
        public const float GravityAcceleration = 0.1f;
        public const float RunSpeed = 0.5f;
        public const float JumpSpeed = 5;

        public InputManager InputManager { get; set; }
        public SystemManager SystemManager { get; set; }
        public TextureManager TextureManager { get; set; }

        private static readonly SpriteSheet Idle = new SpriteSheet("Idle", 7);
        private static readonly SpriteSheet Run = new SpriteSheet("Run", 6);
        private static readonly SpriteSheet Falling = new SpriteSheet("Falling", 5);

        public void Initialize()
        {
            SubscribedUpdateComponents.Add(typeof(Player));
        }

        public void Update(IEditableEntity entity, long frame)
        {
            var player = entity.Get<Player>();
            var inputState = InputManager.GetInputState(player.PlayerID, frame);
            var position = entity.Get<Position>();
            var velocity = entity.Get<Velocity>();

            position = entity.Set(position + velocity);

            bool onGround = SetAboveGroundIfNeeded(entity);

            velocity = entity.Set(velocity.UpdateXVelocity(vx => vx * (onGround ? GroundFriction : AirFriction)));
            if (velocity.X != 0)
            {
                entity.Set(entity.Get<Animated>().SetFlipped(velocity.X < 0));
            }

            switch (player.CurrentState)
            {
                case PlayerState.Idle:
                    entity.Set(Idle);
                    if (inputState.Up.IsDown() || !onGround)
                    {
                        entity.Set(player.SetState(PlayerState.Falling));
                        if (onGround)
                        {
                            entity.Set(velocity.SetYVelocity(JumpSpeed));
                        }
                    }
                    else
                    {
                        if (inputState.Left.IsDown() && inputState.Right.IsUp())
                        {
                            entity.Set(player.SetState(PlayerState.WalkingLeft));
                        }

                        if (inputState.Right.IsDown() && inputState.Left.IsUp())
                        {
                            entity.Set(player.SetState(PlayerState.WalkingRight));
                        }
                    }
                    break;
                case PlayerState.Falling:
                    entity.Set(Falling);

                    if (onGround)
                    {
                        entity.Set(player.SetState(PlayerState.Idle));
                        entity.Set(velocity.SetYVelocity(0));
                    }
                    else
                    {
                        entity.Set(velocity.UpdateYVelocity(vy => vy - GravityAcceleration));
                    }
                    break;
                case PlayerState.WalkingLeft:
                    entity.Set(Run);
                    entity.Set(entity.Get<Animated>().SetFlipped(true));
                    entity.Set(velocity.UpdateXVelocity(vx => vx - RunSpeed));

                    if (inputState.Left.IsUp() || inputState.Right.IsDown() || inputState.Up.IsDown())
                    {
                        entity.Set(player.SetState(PlayerState.Idle));
                    }
                    break;
                case PlayerState.WalkingRight:
                    entity.Set(Run);
                    entity.Set(entity.Get<Animated>().SetFlipped(false));
                    entity.Set(velocity.UpdateXVelocity(vx => vx + RunSpeed));

                    if (inputState.Right.IsUp() || inputState.Left.IsDown() || inputState.Up.IsDown())
                    {
                        entity.Set(player.SetState(PlayerState.Idle));
                    }
                    break;
            }
        }

        public void Update(IEditableWorld editableWorld, long frame)
        {
            var inputs = InputManager.GetInputStates(frame);
            foreach (var inputState in inputs)
            {
                if (inputState.Enter.IsPressed())
                {
                    if (!editableWorld.Entities.Any(entity => entity.Has<Player>() && entity.Get<Player>().PlayerID == inputState.PlayerID))
                    {
                        editableWorld.UpdateEntity(new Entity(
                            Position.Zero,
                            new Player(inputState.PlayerID, PlayerState.Idle),
                            Idle,
                            new Velocity(0, 0),
                            new Animated(6, 0, false)
                        ));
                    }
                }
            }
        }

        private bool SetAboveGroundIfNeeded(IEditableEntity entity)
        {
            var position = entity.Get<Position>();
            var height = TextureManager.GetTexture(entity.Get<SpriteSheet>()).Height;

            if ((position.Y - height / 2) <= GroundLevel)
            {
                entity.Set(position.SetY(GroundLevel + height / 2));
                entity.Set(entity.Get<Velocity>().UpdateYVelocity(vy => vy < 0 ? 0 : vy));
                return true;
            }
            return false;
        }
    }
}
