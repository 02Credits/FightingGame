using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FightingGame.Systems.Interfaces
{
    public interface IUnloadedEntitySystem
    {
        List<Type> SubscribedComponentTypes { get; }
        void Unload(Entity entity);
    }
}
