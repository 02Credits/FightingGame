using FightingGame.Components;
using FightingGame.Interfaces;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FightingGame.Systems
{
    public class TextureManager : IInitializedEntitySystem
    {
        static List<Type> subscribedComponentTypes = new List<Type>
        {
            typeof(Textured),
            typeof(Physics)
        };
        public List<Type> SubscribedComponentTypes { get { return subscribedComponentTypes; } }

        public Dictionary<string, Texture2D> Textures { get; private set; }
        private ContentManager contentManager;
        private GraphicsDevice graphics;

        public TextureManager(ContentManager contentManager, GraphicsDevice graphics)
        {
            Textures = new Dictionary<string, Texture2D>();

            this.contentManager = contentManager;
            this.graphics = graphics;
        }

        public void Initialize(Entity entity)
        {
            var texturedComponent = entity.GetComponent<Textured>();
            var texture = LoadTexturesIfNeeded(texturedComponent.Path);
            texturedComponent.Width = texture.Width;
            texturedComponent.Height = texture.Height;
        }

        private Texture2D LoadTexturesIfNeeded(string path)
        {
            Texture2D texture;
            if (!Textures.ContainsKey(path))
            {
                texture = contentManager.Load<Texture2D>(path);
                Textures[path] = texture;
            }
            else
            {
                texture = Textures[path];
            }
            return texture;
        }
    }
}
