#region Using Statements
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FightingGame.Interfaces;
using FightingGame.Systems;
using FightingGame.Components;
#endregion

namespace FightingGame
{
    public class Game : Microsoft.Xna.Framework.Game
    {
        public static void InitialEntities()
        {
            new Entity(
                new Textured { Path = "Idle", FrameCount = 7 },
                new Position { Value = new Vector2(0, 0) },
                new CameraTarget(),
                new Player { },
                new Sprite()
            );
        }

        #region BoilerPlate
        GraphicsDeviceManager graphics;

        public static Random Random { get; set; }
        public static float Time { get; set; }

        public static readonly List<Entity> Entities = new List<Entity>();
        public static readonly Dictionary<Type, object> Systems = new Dictionary<Type, object>();
        public static readonly List<IInitializedSystem> InitializedSystems = new List<IInitializedSystem>();
        public static readonly List<IInitializedEntitySystem> InitializedEntitySystems = new List<IInitializedEntitySystem>();
        public static readonly List<ILoadedSystem> LoadedSystems = new List<ILoadedSystem>();
        public static readonly List<ILoadedEntitySystem> LoadedEntitySystems = new List<ILoadedEntitySystem>();
        public static readonly List<IUpdatedSystem> UpdatedSystems = new List<IUpdatedSystem>();
        public static readonly List<IUpdatedEntitySystem> UpdatedEntitySystems = new List<IUpdatedEntitySystem>();
        public static readonly List<IDrawnSystem> DrawnSystems = new List<IDrawnSystem>();
        public static readonly List<IDrawnEntitySystem> DrawnEntitySystems = new List<IDrawnEntitySystem>();
        public static readonly List<IUnloadedSystem> UnloadedSystems = new List<IUnloadedSystem>();
        public static readonly List<IUnloadedEntitySystem> UnloadedEntitySystems = new List<IUnloadedEntitySystem>();
        public static readonly List<IDeconstructedEntitySystem> DeconstructedEntitySystems = new List<IDeconstructedEntitySystem>();

        public Game()
        {
            graphics = new GraphicsDeviceManager(this);
            var scale = 2;
            var width = 600;
            graphics.PreferredBackBufferWidth = width * scale;
            graphics.PreferredBackBufferHeight = width * scale * 9 / 16;
            Random = new Random();
            Time = 0;
            Content.RootDirectory = @"Content/bin";
        }

        protected override void Initialize()
        {
            AddSystem(new TextureManager(Content, graphics.GraphicsDevice));
            AddSystem(new SpriteRenderer());
            AddSystem(new CameraManager(graphics.GraphicsDevice));
            AddSystem(new PlayerManager());
            AddSystem(new VertexRenderer(graphics.GraphicsDevice));
            AddSystem(new VertexManager());

            InitializeSystems();
            InitialEntities();

            base.Initialize();
        }
        #endregion

        #region SubscriptionPumpers
        protected void InitializeSystems()
        {
            foreach (var system in InitializedSystems)
            {
                system.Initialize();
            }
        }

        protected override void LoadContent()
        {
            foreach (var system in LoadedSystems)
            {
                system.Load();
            }

            foreach (var system in LoadedEntitySystems)
            {
                foreach (var gameObject in Entities.ToList())
                {
                    foreach (var subscribedComponent in system.SubscribedComponentTypes)
                    {
                        if (gameObject.Components.ContainsKey(subscribedComponent))
                        {
                            system.Load(gameObject);
                            break;
                        }
                    }
                }
            }

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            Time += (gameTime.ElapsedGameTime.Milliseconds / 1000f);
            foreach (var system in UpdatedSystems)
            {
                system.Update();
            }

            foreach (var system in UpdatedEntitySystems)
            {
                foreach (var gameObject in Entities.ToList())
                {
                    foreach (var subscribedComponent in system.SubscribedComponentTypes)
                    {
                        if (gameObject.Components.ContainsKey(subscribedComponent))
                        {
                            system.Update(gameObject);
                            break;
                        }
                    }
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            foreach (var system in DrawnSystems)
            {
                system.Draw();
            }

            foreach (var system in DrawnEntitySystems)
            {
                foreach (var gameObject in Entities.ToList())
                {
                    foreach (var subscribedComponent in system.SubscribedComponentTypes)
                    {
                        if (gameObject.Components.ContainsKey(subscribedComponent))
                        {
                            system.Draw(gameObject);
                            break;
                        }
                    }
                }
            }

            base.Draw(gameTime);
        }

        protected override void UnloadContent()
        {
            foreach (var system in UnloadedSystems)
            {
                system.Unload();
            }

            foreach (var system in UnloadedEntitySystems)
            {
                foreach (var gameObject in Entities.ToList())
                {
                    foreach (var subscribedComponent in system.SubscribedComponentTypes)
                    {
                        if (gameObject.Components.ContainsKey(subscribedComponent))
                        {
                            system.Unload(gameObject);
                            break;
                        }
                    }
                }
            }

            base.UnloadContent();
        }
        #endregion

        #region SystemManagement
        public static void AddSystem(object system)
        {
            var initializedSystem = system as IInitializedSystem;
            if (initializedSystem != null)
            {
                InitializedSystems.Add(initializedSystem);
            }

            var initializedEntitySystem = system as IInitializedEntitySystem;
            if (initializedEntitySystem != null)
            {
                InitializedEntitySystems.Add(initializedEntitySystem);
            }

            var loadedSystem = system as ILoadedSystem;
            if (loadedSystem != null)
            {
                LoadedSystems.Add(loadedSystem);
            }
            var loadedEntitySystem = system as ILoadedEntitySystem;
            if (loadedEntitySystem != null)
            {
                LoadedEntitySystems.Add(loadedEntitySystem);
            }

            var updatedSystem = system as IUpdatedSystem;
            if (updatedSystem != null)
            {
                UpdatedSystems.Add(updatedSystem);
            }

            var updatedEntitySystem = system as IUpdatedEntitySystem;
            if (updatedEntitySystem != null)
            {
                UpdatedEntitySystems.Add(updatedEntitySystem);
            }

            var drawnSystem = system as IDrawnSystem;
            if (drawnSystem != null)
            {
                DrawnSystems.Add(drawnSystem);
            }

            var drawnEntitySystem = system as IDrawnEntitySystem;
            if (drawnEntitySystem != null)
            {
                DrawnEntitySystems.Add(drawnEntitySystem);
            }

            var unloadedSystem = system as IUnloadedSystem;
            if (unloadedSystem != null)
            {
                UnloadedSystems.Add(unloadedSystem);
            }

            var unloadedEntitySystem = system as IUnloadedEntitySystem;
            if (unloadedEntitySystem != null)
            {
                UnloadedEntitySystems.Add(unloadedEntitySystem);
            }

            var deconstructedEntitySystem = system as IDeconstructedEntitySystem;
            if (deconstructedEntitySystem != null)
            {
                DeconstructedEntitySystems.Add(deconstructedEntitySystem);
            }

            Systems[system.GetType()] = system;
        }

        public static T GetSystem<T>()
        {
            var type = typeof(T);
            if (Systems.ContainsKey(type))
            {
                return (T)Systems[type];
            }
            else
            {
                return default(T);
            }
        }
        #endregion

        #region EntityManagement
        public static void AddEntity(Entity entity)
        {
            Entities.Add(entity);

            foreach (var system in InitializedEntitySystems)
            {
                foreach (var subscribedComponent in system.SubscribedComponentTypes)
                {
                    if (entity.Components.ContainsKey(subscribedComponent))
                    {
                        system.Initialize(entity);
                        break;
                    }
                }
            }
        }

        public static void InitializeEntityComponent(Entity entity, Type componentType)
        {
            foreach (var system in InitializedEntitySystems)
            {
                foreach (var subscribedComponent in system.SubscribedComponentTypes)
                {
                    if (subscribedComponent == componentType)
                    {
                        system.Initialize(entity);
                        break;
                    }
                }
            }
        }

        public static void RemoveEntity(Entity entity)
        {
            Entities.Remove(entity);

            foreach (var system in DeconstructedEntitySystems)
            {
                foreach (var subscribedComponent in system.SubscribedComponentTypes)
                {
                    if (entity.Components.ContainsKey(subscribedComponent))
                    {
                        system.Deconstruct(entity);
                        break;
                    }
                }
            }
        }
        #endregion
    }
}