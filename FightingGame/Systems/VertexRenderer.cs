using FightingGame.Systems.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FightingGame.Systems
{
    public class VertexRenderer : IDrawnSystem
    {
        GraphicsDevice graphics;
        BasicEffect basicEffect;

        public VertexRenderer(GraphicsDevice graphics)
        {
            this.graphics = graphics;
            this.basicEffect = new BasicEffect(graphics);
        }

        public void Draw()
        {
            graphics.BlendState = BlendState.AlphaBlend;
            graphics.RasterizerState = RasterizerState.CullNone;
            graphics.SamplerStates[0] = SamplerState.PointClamp;
            graphics.DepthStencilState = DepthStencilState.Default;

            basicEffect.VertexColorEnabled = true;
            basicEffect.TextureEnabled = true;

            basicEffect.World = Matrix.CreateWorld(Vector3.Zero, Vector3.Negate(Vector3.UnitZ), Vector3.UnitY);

            var cameraManager = Game.GetSystem<CameraManager>();
            basicEffect.View = cameraManager.View;
            basicEffect.Projection = cameraManager.Projection;

            var vertexManager = Game.GetSystem<VertexManager>();
            foreach (var texture in vertexManager.TextureOrder)
            {
                var manager = vertexManager.Managers[texture];
                basicEffect.Texture = texture;

                if (manager.VertexCount > 0)
                {
                    foreach (var pass in basicEffect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        graphics.DrawUserIndexedPrimitives(
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

            basicEffect.TextureEnabled = false;

            if (vertexManager.LineVertices.Count > 0)
            {
                foreach (var pass in basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    graphics.DrawUserPrimitives(PrimitiveType.LineList, vertexManager.LineVertices.ToArray(), 0, vertexManager.LineVertices.Count / 2);
                }
            }
        }
    }
}
