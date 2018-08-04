using FightingGame.Systems.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FightingGame.Systems
{
    public struct Animated
    {
        public int AnimationSpeed { get; set; }
        public int CurrentFrame { get; set; }
        public bool Flipped { get; set; }
    }

    public class AnimationManager : IUpdatedEntitySystem
    {
        public List<Type> SubscribedComponentTypes => new List<Type>
        {
            typeof(Animated)
        };

        public void Update(Entity entity)
        {
            var sheet = entity.Get<SpriteSheet>();
            var animated = entity.Get<Animated>();

            if (Game.Frame % animated.AnimationSpeed == 0)
            {
                animated.CurrentFrame++;
                if (animated.CurrentFrame >= sheet.FrameCount)
                {
                    animated.CurrentFrame = 0;
                }
                entity.Set(animated);
            }
        }
    }
}
