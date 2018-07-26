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

        public int Width { get; set; }
        public int Height { get; set; }
    }
}
