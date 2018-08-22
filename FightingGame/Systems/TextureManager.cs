using FightingGame.Systems.Interfaces;
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
            typeof(SpriteSheet),
            typeof(Physics),
            typeof(Focusable)
        };
        public List<Type> SubscribedComponentTypes { get { return subscribedComponentTypes; } }

        public Dictionary<string, Texture2D> Textures { get; private set; }
        private GraphicsDevice graphics;

        public TextureManager(GraphicsDevice graphics)
        {
            Textures = new Dictionary<string, Texture2D>();

            this.graphics = graphics;
        }

        public void Initialize(Entity entity)
        {
            if (entity.TryGet<SpriteSheet>(out var spriteSheet))
            {
                LoadTexturesIfNeeded(spriteSheet);
            }

            if (entity.TryGet<Physics>(out var physicsComponent))
            {
                LoadTexturesIfNeeded(physicsComponent.PhysicsSheet);
            }

            if (entity.TryGet<Focusable>(out var focusableComponent))
            {
                LoadTexturesIfNeeded(focusableComponent.FocusedSheet);
                LoadTexturesIfNeeded(focusableComponent.UnfocusedSheet);
            }
        }

        private Texture2D LoadTexturesIfNeeded(SpriteSheet sheet)
        {
            Texture2D texture;
            if (!Textures.ContainsKey(sheet.Path))
            {
                using (var fileStream = new FileStream("Content/" + sheet.Path + ".png", FileMode.Open))
                {
                    texture = Texture2D.FromStream(graphics, fileStream);
                }

                Textures[sheet.Path] = texture;
            }
            else
            {
                texture = Textures[sheet.Path];
            }
            return texture;
        }
    }
}
