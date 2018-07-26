using FightingGame.Components;
using FightingGame.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FightingGame.Systems
{
    public class PhysicsManager : IUpdatedSystem
    {
        public void Update()
        {
            foreach (var entity1 in Game.Entities.Where(entity => entity.HasComponent<Physics>()))
            {
                var entity1Position = entity1.GetComponent<Position>().Value;
                var entity1Textured = entity1.GetComponent<Textured>();
                var entity1Sprite = entity1.GetComponent<Sprite>();
                foreach (var entity2 in Game.Entities.Where(entity => entity.HasComponent<Physics>()))
                {
                    if (entity2 != entity1)
                    {
                        var entity2Position = entity2.GetComponent<Position>().Value;
                        var entity2Textured = entity2.GetComponent<Textured>();
                        var entity2Sprite = entity2.GetComponent<Sprite>();

                        if (Collision(entity1Sprite, entity1Position, entity1Textured, entity2Sprite, entity2Position, entity2Textured))
                        {
                            if (entity1.TryGetComponent<ColorTint>(out var colorTint))
                            {
                                colorTint.Color = Color.Red;
                            }
                        }
                        else
                        {
                            if (entity1.TryGetComponent<ColorTint>(out var colorTint))
                            {
                                colorTint.Color = Color.White;
                            }
                        }
                    }
                }
            }
        }

        public bool Collision(Sprite sprite1, Vector2 position1, Textured textured1, Sprite sprite2, Vector2 position2, Textured textured2)
        {
            Texture2D texture1 = textured1.Texture;
            Texture2D texture2 = textured2.Texture;
            int minX = (int)Math.Max(position1.X, position2.X);
            int maxX = (int)Math.Min(position1.X + textured1.FrameWidth, position2.X + textured2.FrameWidth);
            int minY = (int)Math.Max(position1.Y, position2.Y);
            int maxY = (int)Math.Min(position1.Y + texture1.Height, position2.Y + texture2.Height);

            int xOffset1 = sprite1.CurrentFrame * textured1.FrameWidth;
            int xOffset2 = sprite2.CurrentFrame * textured2.FrameWidth;

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
