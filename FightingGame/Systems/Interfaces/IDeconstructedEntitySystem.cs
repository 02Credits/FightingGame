using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FightingGame.Systems.Interfaces
{
    public interface IDeconstructedEntitySystem
    {
        List<Type> SubscribedComponentTypes { get; }
        void Deconstruct(Entity entity);
    }
}
