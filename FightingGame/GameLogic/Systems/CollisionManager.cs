using FightingGame.GameLogic.Systems.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace FightingGame.GameLogic.Systems
{
    public class CollisionManager : ISystem
    {
        public TextureManager TextureManager { get; set; }

        public IEnumerable<Point> CollisionPoints(Entity entity1, Entity entity2)
        {
            var spriteSheet1 = entity1.Get<SpriteSheet>();
            var spriteSheet2 = entity2.Get<SpriteSheet>();
            var position1 = entity1.Get<Position>();
            var position2 = entity2.Get<Position>();
            var currentFrame1 = entity1.Get<Animated>().CurrentFrame;
            var currentFrame2 = entity2.Get<Animated>().CurrentFrame;

            Texture2D texture1 = TextureManager.GetTexture(spriteSheet1);
            Texture2D texture2 = TextureManager.GetTexture(spriteSheet2);
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
                        if (x > position1.X && x < position1.X + frameWidth1 && y > position1.Y && y < position1.Y + texture1.Height &&
                            x > position2.X && x < position2.X + frameWidth2 && y > position2.Y && y < position1.Y + texture2.Height)
                        {
                            Color color1 = bits1[(x - position1.X + xOffset1) + (y - position1.Y) * texture1.Width];
                            Color color2 = bits2[(x - position2.X + xOffset2) + (y - position2.Y) * texture2.Width];

                            if (color1 != Color.Transparent && color2 != Color.Transparent)
                            {
                                yield return new Point(x, y);
                            }
                        }
                    }
                }
            }
        }
    }
}
