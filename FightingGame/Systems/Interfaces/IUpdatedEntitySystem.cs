using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FightingGame.Systems.Interfaces
{
    public interface IUpdatedEntitySystem
    {
        List<Type> SubscribedComponentTypes { get; }
        void Update(Entity entity);
    }
}
