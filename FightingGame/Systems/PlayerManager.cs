using FightingGame.Networking;
using FightingGame.Systems.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FightingGame.Systems
{
    public struct Player
    {
        public string PlayerId { get; set; }
    }

    public class PlayerManager : IUpdatedEntitySystem, IUpdatedSystem
    {
        static List<Type> subscribedComponentTypes = new List<Type>
        {
            typeof(Player)
        };
        public List<Type> SubscribedComponentTypes { get { return subscribedComponentTypes; } }

        private static readonly SpriteSheet Idle = new SpriteSheet { Path = "Idle", FrameCount = 7 };
        private static readonly SpriteSheet Run = new SpriteSheet { Path = "Run", FrameCount = 6 };

        public void Update(Entity entity)
        {
            var playerId = entity.Get<Player>().PlayerId;

            var inputManager = Game.GetSystem<InputManager>();
            var inputStates = inputManager.GetInputStates();

            if (inputStates.TryGetValue(playerId, out var inputState))
            {
                Log.Information("Player moved.");
                if ((inputState.Left & KeyStatus.Down) != KeyStatus.None)
                {
                    entity.Set(Run);
                    var position = entity.Get<Position>();
                    position.Value -= new Vector2(1, 0);
                    entity.Set(position);
                    var animated = entity.Get<Animated>();
                    animated.Flipped = true;
                    entity.Set(animated);
                }

                if ((inputState.Right & KeyStatus.Down) != KeyStatus.None)
                {
                    entity.Set(Run);
                    var position = entity.Get<Position>();
                    position.Value += new Vector2(1, 0);
                    entity.Set(position);
                    var animated = entity.Get<Animated>();
                    animated.Flipped = false;
                    entity.Set(animated);
                }

                if (((inputState.Left & inputState.Right) & KeyStatus.Up) != KeyStatus.None)
                {
                    entity.Set(Idle);
                }
            }
        }

        public void Update()
        {
            var inputManager = Game.GetSystem<InputManager>();
            var inputStates = inputManager.GetInputStates();

            if (Game.CurrentScreen == Screen.Playing)
            {
                foreach (var playerId in inputStates.Keys)
                {
                    if (!Game.Entities.Any(entity => entity.Has<Player>() && entity.Get<Player>().PlayerId == playerId))
                    {
                        var inputState = inputStates[playerId];
                        if ((inputState.Enter & KeyStatus.Down) != KeyStatus.None)
                        {
                            Game.AddEntity(new Entity(
                                new Position { Value = Vector2.Zero },
                                new Player { PlayerId = playerId },
                                Idle,
                                new Animated { AnimationSpeed = 10, Flipped = false }
                            ));
                        }
                    }
                }
            }
        }
    }
}
