#region Using Statements
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FightingGame.Systems.Interfaces;
using FightingGame.Systems;
using FightingGame.Networking;
using System.Windows.Forms;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using Serilog;
using Serilog.Core;
#endregion

namespace FightingGame
{
    public enum Screen
    {
        Menu,
        Connecting,
        Waiting,
        Playing
    }

    public class Game : Microsoft.Xna.Framework.Game
    {
        public const int BufferSize = 100;
        public const int ScreenWidth = 600;
        public const double ScreenAspectRatio = 9.0 / 16.0;
        public const int ScreenScale = 2;

        public static Screen CurrentScreen { get; private set; }
        public static int Frame { get; private set; }
        public static bool Rewinding { get; private set; }

        public static void Start()
        {
            levelSwitch.MinimumLevel = Serilog.Events.LogEventLevel.Information;
            Clear();
            CurrentScreen = Screen.Menu;
            var hostFocusedSheet = new SpriteSheet { Path = "HostFocused" };
            var hostUnfocusedSheet = new SpriteSheet { Path = "HostUnfocused" };
            var hostButton = new Entity(
                new Position { Value = new Vector2(-ScreenWidth / 4, 0) },
                hostFocusedSheet,
                new Focusable
                {
                    FocusedSheet = hostFocusedSheet,
                    UnfocusedSheet = hostUnfocusedSheet,
                    Focused = true,
                    NextEntities = new Dictionary<Keys, Entity>(),
                    Activate = WaitingForConnection
                }
            );

            var joinFocusedSheet = new SpriteSheet { Path = "JoinFocused" };
            var joinUnfocusedSheet = new SpriteSheet { Path = "JoinUnfocused" };
            var joinButton = new Entity(
                new Position { Value = new Vector2(ScreenWidth / 4, 0) },
                joinUnfocusedSheet,
                new Focusable
                {
                    FocusedSheet = joinFocusedSheet,
                    UnfocusedSheet = joinUnfocusedSheet,
                    Focused = false,
                    NextEntities = new Dictionary<Keys, Entity>(),
                    Activate = Connecting
                }
            );

            var hostNext = hostButton.Get<Focusable>().NextEntities;
            hostNext[Keys.Tab] = joinButton;
            hostNext[Keys.Right] = joinButton;
            var joinNext = joinButton.Get<Focusable>().NextEntities;
            joinNext[Keys.Tab] = hostButton;
            joinNext[Keys.Left] = hostButton;
        }

        public static void Connecting()
        {
            Form prompt = new Form()
            {
                Width = 500,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "Host Address",
                StartPosition = FormStartPosition.CenterScreen
            };
            Label textLabel = new Label() { Left = 50, Top=20, Text="Host Address" };
            TextBox textBox = new TextBox() { Left = 50, Top=50, Width=400 };
            Button confirmation = new Button() { Text = "Ok", Left=350, Width=100, Top=70, DialogResult = DialogResult.OK };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;
            var hostAddress = prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";

            Clear();
            CurrentScreen = Screen.Connecting;
            GetSystem<NetworkManager>().Connect(hostAddress, 8080);
            new Entity(
                new SpriteSheet { Path = "Connecting" },
                new Position { Value = new Vector2(0, 0) },
                new CameraTarget()
            );
        }

        public static void WaitingForConnection()
        {
            Clear();
            CurrentScreen = Screen.Waiting;
            GetSystem<NetworkManager>().Host();
            new Entity(
                new SpriteSheet { Path = "WaitingForConnection" },
                new Position { Value = new Vector2(0, 0) },
                new CameraTarget()
            );
        }

        public static void Play()
        {
            Clear();
            CurrentScreen = Screen.Playing;
        }

        protected override void Initialize()
        {
            AddSystem(new NetworkManager());
            AddSystem(new InputManager(this));
            AddSystem(new AnimationManager());
            AddSystem(new TextureManager(Content, graphics.GraphicsDevice));
            AddSystem(new SpriteRenderer());
            AddSystem(new CameraManager());
            AddSystem(new PlayerManager());
            AddSystem(new VertexRenderer(graphics.GraphicsDevice));
            AddSystem(new VertexManager());
            AddSystem(new PhysicsManager());
            AddSystem(new UIManager());

            InitializeSystems();

            Start();

            base.Initialize();
        }

        #region BoilerPlate
        GraphicsDeviceManager graphics;

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

        private static LoggingLevelSwitch levelSwitch;

        public Game()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = ScreenWidth * ScreenScale;
            graphics.PreferredBackBufferHeight = (int)(ScreenWidth * ScreenScale * ScreenAspectRatio);
            Content.RootDirectory = @"Content/bin";
            TargetElapsedTime = TimeSpan.FromSeconds(1.0f / 60.0f);
            IsFixedTimeStep = true;

            levelSwitch = new LoggingLevelSwitch(Serilog.Events.LogEventLevel.Error);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(levelSwitch)
                .WriteTo.File("c:/dev/FG" + Guid.NewGuid().ToString() + ".log")
                .CreateLogger();
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
                        if (gameObject.Has(subscribedComponent))
                        {
                            system.Load(gameObject);
                            break;
                        }
                    }
                }
            }

            base.LoadContent();
        }

        int _frames = 0;
        int _fpsTime = 0;
        protected override void Update(GameTime gameTime)
        {
            Frame++;
            Log.Information("Frame {FrameNumber}", Frame);

            if (gameTime != null)
            {
                _fpsTime += gameTime.ElapsedGameTime.Milliseconds;
                if (_fpsTime >= 1000)
                {
                    Window.Title = _frames.ToString();
                    _frames = 0;
                    _fpsTime = 0;
                }
            }

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
                        if (gameObject.Has(subscribedComponent))
                        {
                            system.Update(gameObject);
                            break;
                        }
                    }
                }
            }

            foreach (var entity in Entities)
            {
                entity.Tick();
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            _frames++;

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
                        if (gameObject.Has(subscribedComponent))
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
                        if (gameObject.Has(subscribedComponent))
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
        public void ResimulateFrom(int frame)
        {
            int rewindAmount = Frame - frame;

            Log.Information("Rewound {n} frames", rewindAmount);

            foreach (Entity entity in Entities)
            {
                entity.Rewind(rewindAmount);
            }

            Frame = frame;

            Rewinding = true;
            for (int i = 0; i < rewindAmount; i++)
            {
                Update(null);
            }
            Rewinding = false;
        }

        public static void Clear()
        {
            foreach (var entity in Entities.ToList())
            {
                RemoveEntity(entity);
            }
            AddEntity(new Entity(
                new Position { Value = Vector2.Zero },
                new Camera()));
            Frame = 1;
            GetSystem<InputManager>().ClearHistory();
        }

        public static void AddEntity(Entity entity)
        {
            Entities.Add(entity);

            foreach (var system in InitializedEntitySystems)
            {
                foreach (var subscribedComponent in system.SubscribedComponentTypes)
                {
                    if (entity.Has(subscribedComponent))
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
                    if (entity.Has(subscribedComponent))
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