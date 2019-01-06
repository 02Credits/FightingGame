using FightingGame.GameLogic.Systems.Interfaces;

namespace FightingGame.GameLogic.Systems
{
    public class Animated
    {
        public int AnimationSpeed { get; }
        public int CurrentFrame { get; }
        public bool Flipped { get; }

        public Animated(int animationSpeed, int currentFrame, bool flipped)
        {
            AnimationSpeed = animationSpeed;
            CurrentFrame = currentFrame;
            Flipped = flipped;
        }

        public Animated SetAnimationSpeed(int animationSpeed) => new Animated(animationSpeed, CurrentFrame, Flipped);
        public Animated SetCurrentFrame(int currentFrame) => new Animated(AnimationSpeed, currentFrame, Flipped);
        public Animated SetFlipped(bool flipped) => new Animated(AnimationSpeed, CurrentFrame, flipped);
    }

    public class AnimationManager : SystemBase, IInitializableSystem, IEntityUpdaterSystem
    {
        public SystemManager SystemManager { get; set; }
        public InputManager InputManager { get; set; }
        public PlayerManager PlayerManager { get; set; }

        public void Initialize()
        {
            SubscribedUpdateComponents.Add(typeof(Animated));
            EntityUpdateAfterDependencies.Add(PlayerManager);
        }

        public void Update(IEditableEntity entity, long frame)
        {
            var sheet = entity.Get<SpriteSheet>();
            var animated = entity.Get<Animated>();

            if (frame % animated.AnimationSpeed == 0)
            {
                animated = animated.SetCurrentFrame(animated.CurrentFrame + 1);
                if (animated.CurrentFrame >= sheet.FrameCount)
                {
                    animated = animated.SetCurrentFrame(0);
                }
                entity.Set(animated);
            }
        }
    }
}
