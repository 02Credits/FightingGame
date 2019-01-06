using System;
using System.Collections.Generic;

namespace FightingGame.GameLogic.Systems.Interfaces
{
    public interface IEntityUnloaderSystem
    {
        List<IEntityUnloaderSystem> EntityUnloadBeforeDependencies { get; }
        List<IEntityUnloaderSystem> EntityUnloadAfterDependencies { get; }
        List<Type> SubscribedUnloadComponents { get; }
        void Unload(Entity entity);
    }
}
