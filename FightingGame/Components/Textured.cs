using FightingGame.Systems;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FightingGame.Components
{
    public class Textured : Component
    {
        public string Path { get; set; }
        public int FrameCount { get; set; }

        public Texture2D Texture => Game.GetSystem<TextureManager>().Textures[Path];
        public int FrameWidth => Texture.Width / FrameCount;
    }
}
