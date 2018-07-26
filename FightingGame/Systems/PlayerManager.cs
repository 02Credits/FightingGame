using FightingGame.Components;
using FightingGame.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FightingGame.Systems
{
    public class PlayerManager : IUpdatedEntitySystem
    {
        static List<Type> subscribedComponentTypes = new List<Type>
        {
            typeof(Player)
        };
        public List<Type> SubscribedComponentTypes { get { return subscribedComponentTypes; } }

        int frameCount = 0;
        public void Update(Entity entity)
        {
            var sprite = entity.GetComponent<Sprite>();
            var textured = entity.GetComponent<Textured>();
            sprite.CurrentFrame = frameCount / 5 % textured.FrameCount;
            frameCount++;
        }
    }
}
