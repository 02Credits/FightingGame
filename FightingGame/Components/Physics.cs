using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FightingGame.Components
{
    public class Physics : Component
    {
        static List<Type> requiredComponents = new List<Type>
        {
            typeof(Position),
            typeof(Velocity),
            typeof(Sprite)
        };
        public override List<Type> RequiredComponents { get { return requiredComponents; } }

        public string PhysicsPath { get; set; }
        public bool Static { get; set; }
    }
}
