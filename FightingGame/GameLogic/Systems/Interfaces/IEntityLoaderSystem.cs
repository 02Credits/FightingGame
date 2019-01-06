using System;
using System.Collections.Generic;

namespace FightingGame.GameLogic.Systems.Interfaces
{
    public interface IEntityLoaderSystem
    {
        List<IEntityLoaderSystem> EntityLoadBeforeDependencies { get; }
        List<IEntityLoaderSystem> EntityLoadAfterDependencies { get; }
        List<Type> SubscribedLoadComponents { get; }
        void Load(IEditableEntity entity);
    }
}
