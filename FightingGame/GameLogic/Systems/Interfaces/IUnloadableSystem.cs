using System.Collections.Generic;

namespace FightingGame.GameLogic.Systems.Interfaces
{
    public interface IUnloadableSystem
    {
        List<IUnloadableSystem> UnloadBeforeDependencies { get; }
        List<IUnloadableSystem> UnloadAfterDependencies { get; }
        void Unload();
    }
}
