using FightingGame.Networking;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Utils;

namespace FightingGame.GameLogic
{
    public interface IEntity
    {
        long ID { get; }
        IDictionary<Type, object> State { get; }
    }

    public interface IEditableEntity : IEntity
    {
        T Set<T>(T component);
        void Remove<T>();
    }

    public class Entity : IEntity
    {
        private static IDManager EntityIDManager = new IDManager();

        private readonly ImmutableDictionary<Type, object> _state;

        public long ID { get; }
        public IDictionary<Type, object> State => _state;

        public Entity(params object[] components)
        {
            var stateBuilder = ImmutableDictionary.CreateBuilder<Type, object>();

            foreach (object component in components)
            {
                stateBuilder.Add(component.GetType(), component);
            }

            ID = EntityIDManager.GetNextID();
            _state = stateBuilder.ToImmutable();
        }

        private Entity(long id, ImmutableDictionary<Type, object> state)
        {
            ID = id;
            _state = state;
        }

        public Entity Update(Action<IEditableEntity> update)
        {
            var entityBuilder = new EntityBuilder(this);
            update(entityBuilder);
            return entityBuilder.ToImmutable();
        }

        public Entity Set<T>(T component) => Update(self => self.Set(component));

        private class EntityBuilder : IEditableEntity
        {
            private Entity _previousEntity;
            private ImmutableDictionary<Type, object>.Builder _state;
            public IDictionary<Type, object> State => _state;

            public long ID => _previousEntity.ID;

            public EntityBuilder(Entity previousEntity)
            {
                _previousEntity = previousEntity;
                _state = previousEntity._state.ToBuilder();
            }

            public T Set<T>(T component)
            {
                var type = component.GetType();
                _state[type] = component;
                return component;
            }

            public void Remove<T>() => _state.Remove(typeof(T));

            public Entity ToImmutable() => new Entity(_previousEntity.ID, _state.ToImmutable());
        }
    }

    public static class EntityExtensions
    {
        public static bool Has<T>(this IEntity entity) => entity.Has(typeof(T));
        public static bool Has(this IEntity entity, Type componentType) => entity.State.ContainsKey(componentType);

        public static T Get<T>(this IEntity entity) => (T)entity.State[typeof(T)];
        public static bool TryGet<T>(this IEntity entity, out T component)
        {
            component = default(T);
            if (entity.Has<T>())
            {
                component = entity.Get<T>();
                return true;
            }
            return false;
        }

        public static bool TryRemove<T>(this IEditableEntity entity)
        {
            if (entity.Has<T>())
            {
                entity.Remove<T>();
                return true;
            }
            return false;
        }
    }
}
