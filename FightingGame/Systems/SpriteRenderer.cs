using FightingGame.Components;
using FightingGame.Interfaces;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FightingGame.Systems
{
    public class SpriteRenderer : IDrawnEntitySystem
    {
        static List<Type> subscribedComponentTypes = new List<Type>
        {
            typeof(Sprite)
        };
        public List<Type> SubscribedComponentTypes { get { return subscribedComponentTypes; } }

        public void Draw(Entity entity)
        {
            var sprite = entity.GetComponent<Sprite>();
            var transform = entity.GetComponent<Position>();
            var position = transform.Value;
            var texturedComponent = entity.GetComponent<Textured>();
            var texture = Game.GetSystem<TextureManager>().Textures[texturedComponent.Path];
            var frameCount = texturedComponent.FrameCount;
            var color = entity.HasComponent<ColorTint>() ? entity.GetComponent<ColorTint>().Color : Color.White;

            var translationMatrix = Matrix.CreateTranslation(new Vector3((int)position.X, (int)position.Y, 0));

            var widthOverTwo = texture.Width / frameCount / 2.0;
            var heightOverTwo = texture.Height / 2.0;

            var p0 = Vector3.Transform(new Vector3((int)-widthOverTwo, (int)-heightOverTwo, 0), translationMatrix);
            var p1 = Vector3.Transform(new Vector3((int)widthOverTwo, (int)-heightOverTwo, 0), translationMatrix);
            var p2 = Vector3.Transform(new Vector3((int)widthOverTwo, (int)heightOverTwo, 0), translationMatrix);
            var p3 = Vector3.Transform(new Vector3((int)-widthOverTwo, (int)heightOverTwo, 0), translationMatrix);

            var textureWidth = 1.0f / frameCount;
            var textureOffset = sprite.CurrentFrame * textureWidth;

            var t0 = new Vector2(textureOffset, 1);
            var t1 = new Vector2(textureOffset + textureWidth, 1);
            var t2 = new Vector2(textureOffset + textureWidth, 0);
            var t3 = new Vector2(textureOffset, 0);

            Game.GetSystem<VertexManager>().AddRectangle(texture, color,
                p0, p1, p2, p3,
                t0, t1, t2, t3);
        }
    }
}
