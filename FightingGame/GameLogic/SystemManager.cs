using FightingGame.GameLogic.Systems.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FightingGame.GameLogic
{
    public class SystemManager
    {
        public const int BufferSize = 3600;

        private readonly Dictionary<long, World> _worldHistory = new Dictionary<long, World>();
        public IReadOnlyDictionary<long, World> WorldHistory => _worldHistory;

        private long _latestFrame;

        public IReadOnlyList<IInitializableSystem> InitializableSystems { get; private set; }
        public IReadOnlyList<ILoadableSystem> LoadableSystems { get; private set; }
        public IReadOnlyList<IEntityLoaderSystem> EntityLoaderSystems { get; private set; }
        public IReadOnlyList<IUpdatableSystem> UpdatableSystems { get; private set; }
        public IReadOnlyList<IEntityUpdaterSystem> EntityUpdaterSystems { get; private set; }
        public IReadOnlyList<IDrawableSystem> DrawableSystems { get; private set; }
        public IReadOnlyList<IEntityDrawerSystem> EntityDrawerSystems { get; private set; }
        public IReadOnlyList<IUnloadableSystem> UnloadableSystems { get; private set; }
        public IReadOnlyList<IEntityUnloaderSystem> EntityUnloaderSystems { get; private set; }

        public IList<ISystem> Systems { get; set; }

        public InputManager InputManager { get; set; }

        public string WindowText { get; set; }

        public void Initialize()
        {
            _latestFrame = 0;
            _worldHistory[_latestFrame] = new World(this);

            InitializableSystems = Systems.OfType<IInitializableSystem>().ToList();

            foreach (var system in InitializableSystems)
            {
                system.Initialize();
            }

            LoadableSystems = ResolveOrderDependencies(
                Systems.OfType<ILoadableSystem>(), 
                system => system.LoadBeforeDependencies,
                system => system.LoadAfterDependencies);
            EntityLoaderSystems = ResolveOrderDependencies(
                Systems.OfType<IEntityLoaderSystem>(), 
                system => system.EntityLoadBeforeDependencies,
                system => system.EntityLoadAfterDependencies);
            UpdatableSystems = ResolveOrderDependencies(
                Systems.OfType<IUpdatableSystem>(), 
                system => system.UpdateBeforeDependencies,
                system => system.UpdateAfterDependencies);
            EntityUpdaterSystems = ResolveOrderDependencies(
                Systems.OfType<IEntityUpdaterSystem>(), 
                system => system.EntityUpdateBeforeDependencies,
                system => system.EntityUpdateBeforeDependencies);
            DrawableSystems = ResolveOrderDependencies(
                Systems.OfType<IDrawableSystem>(), 
                system => system.DrawBeforeDependencies,
                system => system.DrawAfterDependencies);
            EntityDrawerSystems = ResolveOrderDependencies(
                Systems.OfType<IEntityDrawerSystem>(), 
                system => system.EntityDrawBeforeDependencies,
                system => system.EntityDrawAfterDependencies);
            UnloadableSystems = ResolveOrderDependencies(
                Systems.OfType<IUnloadableSystem>(), 
                system => system.UnloadBeforeDependencies,
                system => system.UnloadAfterDependencies);
            EntityUnloaderSystems = ResolveOrderDependencies(
                Systems.OfType<IEntityUnloaderSystem>(), 
                system => system.EntityUnloadBeforeDependencies,
                system => system.EntityUnloadAfterDependencies);
        }

        private bool _clearQueued = false;
        public void QueueHistoryClear()
        {
            _clearQueued = true;
        }

        private long? _queuedResimulationFrame = null;
        public void QueueResimulationFrom(long frame)
        {
            if (_queuedResimulationFrame == null || frame < _queuedResimulationFrame)
            {
                _queuedResimulationFrame = frame;
            }
        }

        public void Load()
        {
            foreach (var system in LoadableSystems)
            {
                _worldHistory[_latestFrame] = _worldHistory[_latestFrame].UpdateWorld(system.Load);
            }
        }

        public Entity Load(Entity entity)
        {
            return entity.Update(editableEntity =>
            {
                foreach (var system in EntityLoaderSystems)
                {
                    if (system.SubscribedLoadComponents.Any(entity.Has))
                    {
                        system.Load(editableEntity);
                    }
                }
            });
        }

        public void Update()
        {
            _latestFrame++;
            var frameToRewindTo = InputManager.EarliestFrameUpdate(_latestFrame);
            WindowText = $" Frames Ahead: {_latestFrame - frameToRewindTo}";

            for (long simulatedFrame = frameToRewindTo; simulatedFrame <= _latestFrame; simulatedFrame++)
            {
                _worldHistory[simulatedFrame] = _worldHistory[simulatedFrame - 1];
                foreach (var system in UpdatableSystems)
                {
                    _worldHistory[simulatedFrame] = _worldHistory[simulatedFrame].UpdateWorld(editableWorld =>
                    {
                        system.Update(editableWorld, simulatedFrame);
                    });
                }

                if (_clearQueued)
                {
                    _clearQueued = false;
                    _queuedResimulationFrame = null;

                    World clearedState = _worldHistory[simulatedFrame];
                    InputManager.ClearHistory();
                    _worldHistory.Clear();
                    _worldHistory[0] = clearedState;
                    _latestFrame = 0;
                    break;
                }
            }

            long oldFrame = _latestFrame - BufferSize;
            if (_worldHistory.ContainsKey(oldFrame))
            {
                _worldHistory.Remove(oldFrame);
            }
        }

        public void Draw()
        {
            foreach (var system in DrawableSystems)
            {
                system.Draw(_worldHistory[_latestFrame], _latestFrame);
            }
        }

        public void Unload()
        {
            foreach (var system in UnloadableSystems)
            {
                system.Unload();
            }
        }

        public void Unload(Entity entity)
        {
            foreach (var system in EntityUnloaderSystems)
            {
                system.Unload(entity);
            }
        }

        private IReadOnlyList<T> ResolveOrderDependencies<T>(IEnumerable<T> systems, Func<T, IReadOnlyList<T>> beforeDependencies, Func<T, IReadOnlyList<T>> afterDependencies)
        {
            List<T> orderedSystems = systems.ToList();

            for (int i = 0; i < 20; i++)
            {
                bool reordered = false;

                foreach (var system in orderedSystems.ToList())
                {
                    var afterDependenciesList = afterDependencies(system);
                    if (afterDependenciesList.Any())
                    {
                        var farthestIndex = afterDependenciesList.Max(dep => orderedSystems.IndexOf(dep));
                        if (orderedSystems.IndexOf(system) < farthestIndex)
                        {
                            orderedSystems.Remove(system);
                            orderedSystems.Insert(farthestIndex, system);
                            reordered = true;
                        }
                    }
                }
                foreach (var system in orderedSystems.ToList())
                {
                    var beforeDependenciesList = beforeDependencies(system);
                    if (beforeDependenciesList.Any())
                    {
                        var earliestIndex = beforeDependenciesList.Min(dep => orderedSystems.IndexOf(dep));
                        if (orderedSystems.IndexOf(system) >= earliestIndex)
                        {
                            orderedSystems.Remove(system);
                            orderedSystems.Insert(earliestIndex, system);
                            reordered = true;
                        }
                    }
                }

                if (!reordered)
                {
                    var resultingOrder = orderedSystems.Cast<T>().ToList();
                    Console.WriteLine($"Order for type: {typeof(T).Name}");

                    foreach (var system in resultingOrder)
                    {
                        var systemText = new StringBuilder();
                        systemText.Append($"    {system.GetType().Name}");

                        var beforeDependenciesList = beforeDependencies(system);
                        var afterDependenciesList = afterDependencies(system);

                        if (beforeDependenciesList.Any() || afterDependenciesList.Any())
                        {
                            systemText.Append(" {");
                            if (beforeDependenciesList.Any())
                            {
                                systemText.Append("Before: [ ");
                                systemText.Append(string.Join(", ", beforeDependenciesList.Select(dep => dep.GetType().Name)));
                                systemText.Append(" ]");
                            }

                            if (afterDependenciesList.Any())
                            {
                                if (beforeDependenciesList.Any()) systemText.Append(", ");
                                systemText.Append("After: [ ");
                                systemText.Append(string.Join(", ", afterDependenciesList.Select(dep => dep.GetType().Name)));
                                systemText.Append(" ]");
                            }
                            systemText.Append("}");
                        }

                        Console.WriteLine(systemText);
                    }
                    return resultingOrder;
                }
            }

            throw new ArgumentException("Could not resolve ordering");
        }
    }
}
