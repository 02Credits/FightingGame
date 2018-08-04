using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FightingGame
{
    public class Entity
    {
        private Dictionary<Type, object>[] _layers { get; }
        private int _topLayerIndex;

        public Entity(params object[] components)
        {
            _layers = new Dictionary<Type, object>[Game.BufferSize];
            for (int i = 0; i < Game.BufferSize; i++)
            {
                _layers[i] = new Dictionary<Type, object>();
            }

            _topLayerIndex = Game.BufferSize - 1;

            foreach (var component in components)
            {
                Set(component);
            }

            Game.AddEntity(this);
        }

        private IEnumerable<Dictionary<Type, object>> LayersInOrder()
        {
            int currentIndex = _topLayerIndex;
            do
            {
                yield return _layers[currentIndex];
                currentIndex--;
                if (currentIndex < 0) currentIndex = Game.BufferSize - 1;
            } while (currentIndex != _topLayerIndex);
        }

        public void Tick()
        {
            _topLayerIndex++;
            if (_topLayerIndex == Game.BufferSize) _topLayerIndex = 0;
            Dictionary<Type, object> shedLayer = _layers[_topLayerIndex];

            int nextLayerIndex = _topLayerIndex + 1;
            if (nextLayerIndex == Game.BufferSize) nextLayerIndex = 0;
            Dictionary<Type, object> baseLayer = _layers[nextLayerIndex];

            foreach (Type componentType in shedLayer.Keys)
            {
                if (!baseLayer.ContainsKey(componentType))
                {
                    baseLayer[componentType] = shedLayer[componentType];
                }
            }
            shedLayer.Clear();
        }

        public void Rewind(int frames)
        {
            for (int i = 0; i < frames; i++)
            {
                _layers[_topLayerIndex].Clear();
                _topLayerIndex--;
                if (_topLayerIndex < 0) _topLayerIndex = Game.BufferSize - 1;
            }
        }

        public bool Has<T>() => Has(typeof(T));
        public bool Has(Type componentType)
        {
            foreach (Dictionary<Type, object> layer in LayersInOrder())
            {
                if (layer.ContainsKey(componentType))
                {
                    return true;
                }
            }
            return false;
        }

        public T Get<T>()
        {
            var type = typeof (T);
            foreach (Dictionary<Type, object> layer in LayersInOrder())
            {
                if (layer.ContainsKey(type))
                {
                    return (T)layer[type];
                }
            }
            throw new Exception("Entity does not contain component of type " + typeof(T).Name);
        }

        public bool TryGet<T>(out T component)
        {
            component = default(T);
            if (Has<T>())
            {
                component = Get<T>();
                return true;
            }
            return false;
        }

        public void Set<T>(T component)
        {
            var type = component.GetType();

            _layers[_topLayerIndex][type] = component;

            Game.InitializeEntityComponent(this, type);
        }
    }
}
