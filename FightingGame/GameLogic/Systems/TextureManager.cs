using FightingGame.GameLogic.Systems.Interfaces;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;

namespace FightingGame.GameLogic.Systems
{
    public class TextureManager : SystemBase, IInitializableSystem, ILoadableSystem
    {
        public Dictionary<string, Texture2D> Textures { get; private set; }
        public GraphicsDevice graphics { get; set; }

        public void Initialize()
        {
            Textures = new Dictionary<string, Texture2D>();
        }

        public void Load(IEditableWorld _)
        {
            var textureFiles = Directory.GetFiles("GameLogic/Content/", "*.png");
            foreach (string texture in textureFiles)
            {
                var textureName = Path.GetFileNameWithoutExtension(texture);
                using (var fileStream = new FileStream(texture, FileMode.Open, FileAccess.Read))
                {
                    Textures[textureName] = Texture2D.FromStream(graphics, fileStream);
                }
            }
        }

        public Texture2D GetTexture(SpriteSheet sheet)
        {
            return Textures[sheet.Path];
        }
    }
}
