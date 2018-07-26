using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FightingGame.Components
{
    public class Player : Component
    {
        static List<Type> requiredComponents = new List<Type>
        {
            typeof(Position)
        };
        public override List<Type> RequiredComponents { get { return requiredComponents; } }
    }
}
