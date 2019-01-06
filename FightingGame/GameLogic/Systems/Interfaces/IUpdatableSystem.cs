using System.Collections.Generic;

namespace FightingGame.GameLogic.Systems.Interfaces
{
    public interface IUpdatableSystem
    {
        List<IUpdatableSystem> UpdateBeforeDependencies { get; }
        List<IUpdatableSystem> UpdateAfterDependencies { get; }
        void Update(IEditableWorld world, long frame);
    }
}
