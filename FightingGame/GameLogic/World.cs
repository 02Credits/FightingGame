using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace FightingGame.GameLogic
{
    public interface IWorld
    {
        IEnumerable<Entity> Entities { get; }
    }

    public interface IEditableWorld : IWorld
    {
        Entity UpdateEntity(Entity entity);
        void RemoveEntity(Entity entity);
        void Clear();
    }

    public class World : IWorld
    {
        private ImmutableDictionary<long, Entity> _entities;
        private SystemManager _systemManager;

        public IEnumerable<Entity> Entities => _entities.Values;

        public World(SystemManager systemManager)
        {
            _entities = ImmutableDictionary<long, Entity>.Empty;
            _systemManager = systemManager;
        }

        private World(ImmutableDictionary<long, Entity> entities, SystemManager systemManager)
        {
            _entities = entities;
            _systemManager = systemManager;
        }

        public World UpdateWorld(Action<IEditableWorld> update)
        {
            var worldBuilder = new WorldBuilder(this, _entities, _systemManager);
            update(worldBuilder);
            return worldBuilder.ToImmutable();
        }

        private class WorldBuilder : IEditableWorld
        {
            private World _previousWorld;

            private ImmutableDictionary<long, Entity>.Builder _entities;
            public IEnumerable<Entity> Entities => _entities.Values;

            private HashSet<long> _addedEntities = new HashSet<long>();
            private HashSet<Entity> _removedEntities = new HashSet<Entity>();

            private SystemManager _systemManager;

            private bool _anythingChanged = false;

            public WorldBuilder(World previousWorld, ImmutableDictionary<long, Entity> entities, SystemManager systemManager)
            {
                _previousWorld = previousWorld;
                _entities = entities.ToBuilder();
                _systemManager = systemManager;
            }

            public Entity UpdateEntity(Entity entity)
            {
                _anythingChanged = true;
                if (!_entities.ContainsKey(entity.ID))
                {
                    _entities.Add(entity.ID, entity);
                    _addedEntities.Add(entity.ID);
                }
                else
                {
                    _entities.Remove(entity.ID);
                    _entities.Add(entity.ID, entity);
                }

                if (_removedEntities.Contains(entity))
                {
                    _removedEntities.Remove(entity);
                }

                return entity;
            }

            public void RemoveEntity(Entity entity)
            {
                if (_entities.ContainsKey(entity.ID))
                {
                    _anythingChanged = true;
                    _entities.Remove(entity.ID);
                    _removedEntities.Add(entity);
                }

                if (_addedEntities.Contains(entity.ID))
                {
                    _addedEntities.Remove(entity.ID);
                }
            }

            public void Clear()
            {
                foreach (Entity entity in _entities.Values.ToList())
                {
                    RemoveEntity(entity);
                }
            }

            public World ToImmutable()
            {
                if (!_anythingChanged) return _previousWorld;

                foreach (long entityID in _addedEntities)
                {
                    if (!_previousWorld._entities.ContainsKey(entityID))
                    {
                        var entity = _entities[entityID];
                        _entities.Remove(entityID);
                        var resultingEntity = _systemManager.Load(entity);
                        if (resultingEntity != null)
                        {
                            _entities.Add(entityID, resultingEntity);
                        }
                    }
                }

                foreach (Entity entity in _removedEntities)
                {
                    _systemManager.Unload(entity);
                }

                return new World(_entities.ToImmutable(), _systemManager);
            }
        }
    }
}
