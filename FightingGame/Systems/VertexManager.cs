using FightingGame.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace FightingGame.Systems
{
    public class VertexManager : IDrawnSystem
    {
        public Dictionary<Texture2D, TextureVertexManager> Managers = new Dictionary<Texture2D, TextureVertexManager>();
        public List<Texture2D> TextureOrder = new List<Texture2D>();
        public List<VertexPositionColor> LineVertices = new List<VertexPositionColor>();

        private TextureVertexManager GetManager(Texture2D texture)
        {
            TextureVertexManager returnManager = null;
            Managers.TryGetValue(texture, out returnManager);
            if (returnManager == null)
            {
                returnManager = new TextureVertexManager();
                Managers[texture] = returnManager;
            }

            if (!TextureOrder.Contains(texture))
            {
                TextureOrder.Add(texture);
            }
            return returnManager;
        }

        public void AddVertex(Texture2D texture, Vector3 position, Vector2 texturePosition, Color color)
        {
            GetManager(texture).AddVertex(position, texturePosition, color);
        }

        public void AddRectangle(Texture2D texture, Color color,
            Vector3 topLeftWorld, Vector3 topRightWorld, Vector3 bottomRightWorld, Vector3 bottomLeftWorld,
            Vector2 topLeftTexture, Vector2 topRightTexture, Vector2 bottomRightTexture, Vector2 bottomLeftTexture)
        {
            var manager = GetManager(texture);
            manager.AddRectangle(color,
                topLeftWorld, topRightWorld, bottomRightWorld, bottomLeftWorld,
                topLeftTexture, topRightTexture, bottomRightTexture, bottomLeftTexture);
        }

        public void AddPolygon(Texture2D texture, Color color, int count,
            IEnumerable<Vector3> worldList, IEnumerable<Vector2> textureList)
        {
            var manager = GetManager(texture);
            manager.AddPolygon(color, count, worldList, textureList);
        }

        [Conditional("DEBUG")]
        public void AddDebugLine(Vector2 p1, Vector2 p2, Color color)
        {
            AddDebugLine(p1, p2, color, Matrix.Identity);
        }

        [Conditional("DEBUG")]
        public void AddDebugLine(Vector2 p1, Vector2 p2, Color color, Matrix transform)
        {
            VertexPositionColor vert;
            vert.Position = Vector3.Transform(new Vector3(p1, 0), transform);
            vert.Color = color;
            LineVertices.Add(vert);
            vert.Position = Vector3.Transform(new Vector3(p2, 0), transform);
            LineVertices.Add(vert);
        }

        public void Draw()
        {
            foreach (var value in Managers.Values)
            {
                value.Clear();
                TextureOrder.Clear();
            }
            LineVertices.Clear();
        }

        public class TextureVertexManager
        {
            public static VertexPositionColorTexture vertexToAdd;
            public VertexPositionColorTexture[] Vertices = new VertexPositionColorTexture[4000];
            public short[] Indices = new short[4000];

            public int VertexCount { get; private set; }

            public int IndexCount { get; private set; }

            private short CurrentIndex { get; set; }

            private int CurrentMaxVertexCount { get; set; }

            private int CurrentMaxIndexCount { get; set; }

            public void AddRectangle(Color color,
                Vector3 topLeftWorld, Vector3 topRightWorld, Vector3 bottomRightWorld, Vector3 bottomLeftWorld,
                Vector2 topLeftTexture, Vector2 topRightTexture, Vector2 bottomRightTexture, Vector2 bottomLeftTexture)
            {
                AddVertex(topLeftWorld, topLeftTexture, color);
                AddVertex(topRightWorld, topRightTexture, color);
                AddVertex(bottomRightWorld, bottomRightTexture, color);
                AddVertex(bottomLeftWorld, bottomLeftTexture, color);
                AddIndex(CurrentIndex);
                AddIndex((short)(CurrentIndex + 1));
                AddIndex((short)(CurrentIndex + 2));
                AddIndex((short)(CurrentIndex + 2));
                AddIndex((short)(CurrentIndex + 3));
                AddIndex(CurrentIndex);

                CurrentIndex += 4;
            }

            public void AddPolygon(Color color, int count,
                IEnumerable<Vector3> worldVectors,
                IEnumerable<Vector2> textureVectors)
            {
                // Uses IEnumerables because then I dont have to cast to a list (basically one less loop over the list)
                var worldEnumerator = worldVectors.GetEnumerator();
                var textureEnumerator = textureVectors.GetEnumerator();
                while (worldEnumerator.MoveNext())
                {
                    textureEnumerator.MoveNext();
                    AddVertex(worldEnumerator.Current, textureEnumerator.Current, color);
                }

                for (int i = 1; i < count - 1; i++)
                {
                    AddIndex(CurrentIndex);
                    AddIndex((short)(CurrentIndex + i));
                    AddIndex((short)(CurrentIndex + i + 1));
                }

                CurrentIndex += (short)count;
            }

            public void AddVertex(Vector3 position, Vector2 texturePosition, Color color)
            {
                VertexCount += 1;

                ExtendVertexArrayIfNeeded();

                vertexToAdd.Position = position;
                vertexToAdd.TextureCoordinate = texturePosition;
                vertexToAdd.Color = color;
                Vertices[VertexCount - 1] = vertexToAdd;
            }

            public void AddIndex(short index)
            {
                IndexCount += 1;

                ExtendIndexArrayIfNeeded();

                Indices[IndexCount - 1] = index;
            }

            private void ExtendVertexArrayIfNeeded()
            {
                if (VertexCount > CurrentMaxVertexCount)
                {
                    CurrentMaxVertexCount = VertexCount;
                    if (CurrentMaxVertexCount > Vertices.Length)
                    {
                        var newArray = new VertexPositionColorTexture[CurrentMaxVertexCount + 500];
                        Vertices.CopyTo(newArray, 0);
                        Vertices = newArray;
                    }
                }
            }

            private void ExtendIndexArrayIfNeeded()
            {
                if (IndexCount > CurrentMaxIndexCount)
                {
                    CurrentMaxIndexCount = IndexCount;
                    if (CurrentMaxIndexCount > Indices.Length)
                    {
                        var newArray = new Int16[CurrentMaxIndexCount + 500];
                        Indices.CopyTo(newArray, 0);
                        Indices = newArray;
                    }
                }
            }

            public void Clear()
            {
                VertexCount = 0;
                IndexCount = 0;
                CurrentIndex = 0;
            }
        }
    }
}