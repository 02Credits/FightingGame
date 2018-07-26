using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FightingGame.Components
{
    public abstract class Component
    {
        static List<Type> requiredComponents = new List<Type>();
        public virtual List<Type> RequiredComponents { get { return requiredComponents; } }
    }
}
