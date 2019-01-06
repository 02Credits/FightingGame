using System.Collections.Generic;

namespace FightingGame.GameLogic.Systems.Interfaces
{
    public interface ILoadableSystem
    {
        List<ILoadableSystem> LoadBeforeDependencies { get; }
        List<ILoadableSystem> LoadAfterDependencies { get; }
        void Load(IEditableWorld editableWorld);
    }
}
