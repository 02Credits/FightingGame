using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FightingGame.Interfaces
{
    public interface ILoadedEntitySystem
    {
        List<Type> SubscribedComponentTypes { get; }
        void Load(Entity entity);
    }
}
