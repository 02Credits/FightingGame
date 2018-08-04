using FightingGame.Systems.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FightingGame.Systems
{
    public struct ColorTint
    {
        public Color Color { get; set; }
    }

    public struct Position
    {
        public Vector2 Value { get; set; }
    }

    public struct SpriteSheet
    {
        public string Path { get; set; }
        public int FrameCount { get; set; }

        public SpriteSheet(string path, int frameCount = 1) : this()
        {
            Path = path;
            FrameCount = frameCount;
        }

        public Texture2D Texture => Game.GetSystem<TextureManager>().Textures[Path];
    }

    public class SpriteRenderer : IDrawnEntitySystem
    {
        static List<Type> subscribedComponentTypes = new List<Type>
        {
            typeof(SpriteSheet)
        };
        public List<Type> SubscribedComponentTypes { get { return subscribedComponentTypes; } }

        public void Draw(Entity entity)
        {
            var position = entity.Get<Position>().Value;
            var sprite = entity.Get<SpriteSheet>();
            int frameCount = sprite.FrameCount;
            if (frameCount == 0) frameCount = 1;
            int currentFrame = 0;
            bool flipped = false;

            if (entity.TryGet<Animated>(out var animated))
            {
                currentFrame = animated.CurrentFrame;
                flipped = animated.Flipped;
            }
            var texture = sprite.Texture;
            var color = entity.Has<ColorTint>() ? entity.Get<ColorTint>().Color : Color.White;

            var translationMatrix = Matrix.CreateTranslation(new Vector3((int)position.X, (int)position.Y, 0));

            var widthOverTwo = texture.Width / frameCount / 2.0;
            var heightOverTwo = texture.Height / 2.0;

            var p0 = Vector3.Transform(new Vector3((int)-widthOverTwo, (int)-heightOverTwo, 0), translationMatrix);
            var p1 = Vector3.Transform(new Vector3((int)widthOverTwo, (int)-heightOverTwo, 0), translationMatrix);
            var p2 = Vector3.Transform(new Vector3((int)widthOverTwo, (int)heightOverTwo, 0), translationMatrix);
            var p3 = Vector3.Transform(new Vector3((int)-widthOverTwo, (int)heightOverTwo, 0), translationMatrix);

            var textureWidth = 1.0f / frameCount;
            var textureOffset = currentFrame * textureWidth;

            var t0 = flipped ? new Vector2(1.0f - textureOffset, 1) : new Vector2(textureOffset, 1);
            var t1 = flipped ? new Vector2(1.0f - (textureOffset + textureWidth), 1) : new Vector2(textureOffset + textureWidth, 1);
            var t2 = flipped ? new Vector2(1.0f - (textureOffset + textureWidth), 0) : new Vector2(textureOffset + textureWidth, 0);
            var t3 = flipped ? new Vector2(1.0f - textureOffset, 0) : new Vector2(textureOffset, 0);

            Game.GetSystem<VertexManager>().AddRectangle(texture, color,
                p0, p1, p2, p3,
                t0, t1, t2, t3);
        }
    }
}
