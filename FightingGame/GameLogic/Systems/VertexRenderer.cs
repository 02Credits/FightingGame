using FightingGame.GameLogic.Systems.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FightingGame.GameLogic.Systems
{
    public class VertexRenderer : SystemBase, IInitializableSystem, IDrawableSystem
    {
        public CameraManager CameraManager { get; set; }
        public VertexManager VertexManager { get; set; }
        public SpriteRenderer SpriteRenderer { get; set; }
        public GraphicsDevice Graphics { get; set; }
        public BasicEffect BasicEffect { get; set; }

        public void Initialize()
        {
            DrawAfterDependencies.Add(SpriteRenderer);
        }

        public void Draw(World _world, long frame)
        {
            Graphics.BlendState = BlendState.AlphaBlend;
            Graphics.RasterizerState = RasterizerState.CullNone;
            Graphics.SamplerStates[0] = SamplerState.PointClamp;
            Graphics.DepthStencilState = DepthStencilState.Default;

            BasicEffect.VertexColorEnabled = true;
            BasicEffect.TextureEnabled = true;

            BasicEffect.World = Matrix.CreateWorld(Vector3.Zero, Vector3.Negate(Vector3.UnitZ), Vector3.UnitY);

            BasicEffect.View = CameraManager.View;
            BasicEffect.Projection = CameraManager.Projection;

            foreach (var texture in VertexManager.TextureOrder)
            {
                var manager = VertexManager.Managers[texture];
                BasicEffect.Texture = texture;

                if (manager.VertexCount > 0)
                {
                    foreach (var pass in BasicEffect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        Graphics.DrawUserIndexedPrimitives(
                            PrimitiveType.TriangleList,
                            manager.Vertices,
                            0,
                            manager.VertexCount,
                            manager.Indices,
                            0,
                            manager.IndexCount / 3);
                    }
                }
            }

            BasicEffect.TextureEnabled = false;

            if (VertexManager.LineVertices.Count > 0)
            {
                foreach (var pass in BasicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    Graphics.DrawUserPrimitives(PrimitiveType.LineList, VertexManager.LineVertices.ToArray(), 0, VertexManager.LineVertices.Count / 2);
                }
            }
        }
    }
}
