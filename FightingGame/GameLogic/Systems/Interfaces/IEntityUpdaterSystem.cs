using System;
using System.Collections.Generic;

namespace FightingGame.GameLogic.Systems.Interfaces
{
    public interface IEntityUpdaterSystem
    {
        List<IEntityUpdaterSystem> EntityUpdateBeforeDependencies { get; }
        List<IEntityUpdaterSystem> EntityUpdateAfterDependencies { get; }
        List<Type> SubscribedUpdateComponents { get; }
        void Update(IEditableEntity entity, long frame);
    }
}
