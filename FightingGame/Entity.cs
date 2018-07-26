using FightingGame.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FightingGame
{
    public class Entity
    {
        public Dictionary<Type, Component> Components { get; set; }

        public Entity(params Component[] components)
        {
            Initialize(components.ToList());
        }

        public Entity(List<Component> components)
        {
            Initialize(components);
        }

        public void Initialize(List<Component> components)
        {
            Components = new Dictionary<Type, Component>();

            foreach (var component in components)
            {
                var type = component.GetType();
                foreach (var requirement in component.RequiredComponents)
                {
                    if (!Components.ContainsKey(requirement))
                        throw new ArgumentException(type.Name + " requires " + requirement.Name);
                }

                Components[type] = component;
            }

            Game.AddEntity(this);
        }

        public bool HasComponent<T>()
        {
            var type = typeof (T);
            return Components.ContainsKey(type);
        }

        public T GetComponent<T>()
            where T : Component
        {
            var type = typeof (T);
            if (Components.ContainsKey(type))
            {
                return (T) Components[type];
            }
            else
            {
                return null;
            }
        }

        public bool TryGetComponent<T>(out T component)
            where T : Component
        {
            component = default(T);
            if (HasComponent<T>())
            {
                component = GetComponent<T>();
                return true;
            }
            return false;
        }

        public void AddComponent<T>(T component)
            where T : Component
        {
            var type = component.GetType();

            foreach (var requirement in component.RequiredComponents)
            {
                if (!Components.ContainsKey(requirement))
                    throw new ArgumentException(type.Name + " requires " + requirement.Name);
            }

            Components[type] = component;

            Game.InitializeEntityComponent(this, type);
        }

        public void AddComponents(List<Component> components)
        {
            foreach (var component in components)
            {
                var type = component.GetType();
                foreach (var requirement in component.RequiredComponents)
                {
                    if (!Components.ContainsKey(requirement))
                        throw new ArgumentException(type.Name + " requires " + requirement.Name);
                }

                Components[type] = component;

                Game.InitializeEntityComponent(this, type);
            }
        }
}
}
