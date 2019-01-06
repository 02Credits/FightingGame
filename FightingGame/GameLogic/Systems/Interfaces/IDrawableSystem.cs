using System.Collections.Generic;

namespace FightingGame.GameLogic.Systems.Interfaces
{
    public interface IDrawableSystem
    {
        List<IDrawableSystem> DrawBeforeDependencies { get; }
        List<IDrawableSystem> DrawAfterDependencies { get; }
        void Draw(World world, long frame);
    }
}
