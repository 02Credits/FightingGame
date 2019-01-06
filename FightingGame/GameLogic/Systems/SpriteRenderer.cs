using FightingGame.GameLogic.Systems.Interfaces;
using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace FightingGame.GameLogic.Systems
{
    public class ColorTint
    {
        public Color Color { get; }

        public ColorTint(Color color)
        {
            Color = color;
        }
    }

    public class Position
    {
        public int X { get; }
        public int Y { get; }

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Position SetX(int x) => new Position(x, Y);
        public Position SetY(int y) => new Position(X, y);

        public Position UpdateX(Func<int, int> update) => SetX(update(X));
        public Position UpdateY(Func<int, int> update) => SetY(update(Y));

        public Vector2 ToVector() => new Vector2(X, Y);

        public static readonly Position Zero = new Position(0, 0);

        public static Position operator+ (Position a, Position b)
            => new Position(a.X + b.X, a.Y + b.Y);

        public static Position operator -(Position a, Position b)
            => new Position(a.X - b.X, a.Y - b.Y);
    }

    public class SpriteSheet
    {
        public string Path { get; }
        public int FrameCount { get; }

        public SpriteSheet(string path, int frameCount = 1)
        {
            Path = path;
            FrameCount = frameCount;
        }
    }

    public class SpriteRenderer : SystemBase, IInitializableSystem, IDrawableSystem
    {
        public TextureManager TextureManager { get; set; }
        public VertexManager VertexManager { get; set; }
        public VertexRenderer VertexRenderer { get; set; }
        public AnimationManager AnimationManager { get; set; }
        public SystemManager SystemManager { get; set; }

        public void Initialize()
        {
            DrawAfterDependencies.Add(VertexManager);
            DrawBeforeDependencies.Add(VertexRenderer);
        }

        public void Draw(World world, long frame)
        {
            foreach (Entity entity in world.Entities.Where(entity => entity.Has<SpriteSheet>()))
            {
                var position = entity.Get<Position>();
                var sprite = entity.Get<SpriteSheet>();
                int frameCount = sprite.FrameCount;
                if (frameCount == 0) frameCount = 1;
                int currentAnimationFrame = 0;
                bool flipped = false;

                if (entity.TryGet<Animated>(out var animated))
                {
                    currentAnimationFrame = animated.CurrentFrame;
                    flipped = animated.Flipped;
                }
                var texture = TextureManager.GetTexture(sprite);
                var color = entity.Has<ColorTint>() ? entity.Get<ColorTint>().Color : Color.White;

                var translationMatrix = Matrix.CreateTranslation(new Vector3(position.X, position.Y, 0));

                var widthOverTwo = texture.Width / frameCount / 2.0;
                var heightOverTwo = texture.Height / 2.0;

                var p0 = Vector3.Transform(new Vector3((int)-widthOverTwo, (int)-heightOverTwo, 0), translationMatrix);
                var p1 = Vector3.Transform(new Vector3((int)widthOverTwo, (int)-heightOverTwo, 0), translationMatrix);
                var p2 = Vector3.Transform(new Vector3((int)widthOverTwo, (int)heightOverTwo, 0), translationMatrix);
                var p3 = Vector3.Transform(new Vector3((int)-widthOverTwo, (int)heightOverTwo, 0), translationMatrix);

                var textureWidth = 1.0f / frameCount;
                var textureOffset = currentAnimationFrame * textureWidth;

                var t0 = flipped ? new Vector2(1.0f - textureOffset, 1) : new Vector2(textureOffset, 1);
                var t1 = flipped ? new Vector2(1.0f - (textureOffset + textureWidth), 1) : new Vector2(textureOffset + textureWidth, 1);
                var t2 = flipped ? new Vector2(1.0f - (textureOffset + textureWidth), 0) : new Vector2(textureOffset + textureWidth, 0);
                var t3 = flipped ? new Vector2(1.0f - textureOffset, 0) : new Vector2(textureOffset, 0);

                VertexManager.AddRectangle(texture, color,
                    p0, p1, p2, p3,
                    t0, t1, t2, t3);
            }
        }
    }
}
