using System;
using System.Collections.Generic;

namespace FightingGame.GameLogic.Systems.Interfaces
{
    public interface IEntityDrawerSystem
    {
        List<IEntityDrawerSystem> EntityDrawBeforeDependencies { get; }
        List<IEntityDrawerSystem> EntityDrawAfterDependencies { get; }
        List<Type> SubscribedDrawComponents { get; }
        void Draw(Entity entity, long frame);
    }
}
