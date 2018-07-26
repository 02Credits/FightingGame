using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FightingGame.Components
{
    public class Sprite : Component
    {
        static List<Type> requiredComponents = new List<Type>
        {
            typeof(Position),
            typeof(Textured),
        };

        public override List<Type> RequiredComponents { get { return requiredComponents; } }

        public int CurrentFrame { get; set; }
    }
}
