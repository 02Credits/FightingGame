using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FightingGame.Components
{
    class Velocity : Component
    {
        static List<Type> requiredComponents = new List<Type>
        {
            typeof(Position),
        };

        public override List<Type> RequiredComponents { get { return requiredComponents; } }

        public Vector2 Value { get; set; }
    }
}
