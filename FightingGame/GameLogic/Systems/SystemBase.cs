using FightingGame.GameLogic.Systems.Interfaces;
using System;
using System.Collections.Generic;

namespace FightingGame.GameLogic.Systems
{
    public abstract class SystemBase : ISystem
    {
        public List<Type> SubscribedLoadComponents { get; } = new List<Type>();
        public List<Type> SubscribedDrawComponents { get; } = new List<Type>();
        public List<Type> SubscribedUpdateComponents { get; } = new List<Type>();
        public List<Type> SubscribedUnloadComponents { get; } = new List<Type>();
        
        public List<ILoadableSystem> LoadBeforeDependencies { get; } = new List<ILoadableSystem>();
        public List<ILoadableSystem> LoadAfterDependencies { get; } = new List<ILoadableSystem>();
        public List<IEntityLoaderSystem> EntityLoadBeforeDependencies { get; } = new List<IEntityLoaderSystem>();
        public List<IEntityLoaderSystem> EntityLoadAfterDependencies { get; } = new List<IEntityLoaderSystem>();
        public List<IUpdatableSystem> UpdateBeforeDependencies { get; } = new List<IUpdatableSystem>();
        public List<IUpdatableSystem> UpdateAfterDependencies { get; } = new List<IUpdatableSystem>();
        public List<IEntityUpdaterSystem> EntityUpdateBeforeDependencies { get; } = new List<IEntityUpdaterSystem>();
        public List<IEntityUpdaterSystem> EntityUpdateAfterDependencies { get; } = new List<IEntityUpdaterSystem>();
        public List<IDrawableSystem> DrawBeforeDependencies { get; } = new List<IDrawableSystem>();
        public List<IDrawableSystem> DrawAfterDependencies { get; } = new List<IDrawableSystem>();
        public List<IEntityDrawerSystem> EntityDrawBeforeDependencies { get; } = new List<IEntityDrawerSystem>();
        public List<IEntityDrawerSystem> EntityDrawAfterDependencies { get; } = new List<IEntityDrawerSystem>();
        public List<IUnloadableSystem> UnloadBeforeDependencies { get; } = new List<IUnloadableSystem>();
        public List<IUnloadableSystem> UnloadAfterDependencies { get; } = new List<IUnloadableSystem>();
        public List<IEntityUnloaderSystem> EntityUnloadBeforeDependencies { get; } = new List<IEntityUnloaderSystem>();
        public List<IEntityUnloaderSystem> EntityUnloadAfterDependencies { get; } = new List<IEntityUnloaderSystem>();
    }
}
