using FightingGame.Systems.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FightingGame.Systems
{
    public struct Velocity
    {
        public Vector2 Value { get; set; }
    }

    public struct Physics
    {
        public SpriteSheet PhysicsSheet { get; set; }
        public bool Static { get; set; }
    }

    public class PhysicsManager : IUpdatedSystem
    {
        public void Update()
        {
            foreach (var entity1 in Game.Entities.Where(entity => entity.Has<Physics>()))
            {
                var entity1Position = entity1.Get<Position>().Value;
                var entity1SpriteSheet = entity1.Get<SpriteSheet>();
                var entity1Animated = entity1.Get<Animated>();
                foreach (var entity2 in Game.Entities.Where(entity => entity.Has<Physics>()))
                {
                    if (entity2 != entity1)
                    {
                        var entity2Position = entity2.Get<Position>().Value;
                        var entity2SpriteSheet = entity2.Get<SpriteSheet>();
                        var entity2Animated = entity2.Get<Animated>();

                        if (Collision(entity1SpriteSheet, entity1Animated.CurrentFrame, entity1Position,
                                      entity2SpriteSheet, entity2Animated.CurrentFrame, entity2Position))
                        {
                            //entity1.Set(new ColorTint { Color = Color.Red });
                        }
                        else
                        {
                            //entity1.Set(new ColorTint { Color = Color.White });
                        }
                    }
                }
            }
        }

        public bool Collision(SpriteSheet spriteSheet1, int currentFrame1, Vector2 position1, 
                              SpriteSheet spriteSheet2, int currentFrame2, Vector2 position2)
        {
            Texture2D texture1 = spriteSheet1.Texture;
            Texture2D texture2 = spriteSheet2.Texture;
            int frameWidth1 = texture1.Width / spriteSheet1.FrameCount;
            int frameWidth2 = texture2.Width / spriteSheet2.FrameCount;
            int minX = (int)Math.Max(position1.X, position2.X);
            int maxX = (int)Math.Min(position1.X + frameWidth1, position2.X + frameWidth2);
            int minY = (int)Math.Max(position1.Y, position2.Y);
            int maxY = (int)Math.Min(position1.Y + texture1.Height, position2.Y + texture2.Height);

            int xOffset1 = currentFrame1 * frameWidth1;
            int xOffset2 = currentFrame2 * frameWidth2;

            if (minX < maxX && minY < maxY)
            {
                Color[] bits1 = new Color[texture1.Width * texture1.Height];
                texture1.GetData(bits1);
                Color[] bits2 = new Color[texture2.Width * texture2.Height];
                texture2.GetData(bits2);

                for (int y = minY; y < maxY; y++)
                {
                    for (int x = minX; x < maxX; x++)
                    {
                        Color color1 = bits1[(x - (int)position1.X + xOffset1) + (y - (int)position1.Y) * texture1.Width];
                        Color color2 = bits2[(x - (int)position2.X + xOffset2) + (y - (int)position2.Y) * texture2.Width];

                        if (color1 != Color.Transparent && color2 != Color.Transparent)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
