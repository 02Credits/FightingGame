using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Autofac;
using System.Reflection;
using FightingGame.Networking;
using MonoGame.Framework.WpfInterop;
using MonoGame.Framework.WpfInterop.Input;
using FightingGame.GameLogic.Systems.Interfaces;

namespace FightingGame.GameLogic
{
    public class Game : WpfGame
    {
        public const float ASPECT_RATIO = 2.0f;

        private IContainer _container;
        private ILifetimeScope _gameScope;

        private SystemManager _systemManager;
        private BasicEffect _basicEffect;
        private WpfKeyboard _keyboard;
        private WpfMouse _mouse;

        public Game(IContainer container)
        {
            _container = container;
        }

        protected override void Initialize()
        {
            TargetElapsedTime = TimeSpan.FromSeconds(1.0f / 60.0f);

            _basicEffect = new BasicEffect(GraphicsDevice);
            _keyboard = new WpfKeyboard(this);
            _mouse = new WpfMouse(this);

            _gameScope = _container.BeginLifetimeScope(gameScopeBuilder =>
            {
                var assembly = Assembly.GetExecutingAssembly();
                gameScopeBuilder.RegisterAssemblyTypes(assembly)
                       .As<ISystem>()
                       .AsSelf()
                       .SingleInstance()
                       .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);

                gameScopeBuilder.RegisterType<SystemManager>()
                       .AsSelf()
                       .SingleInstance()
                       .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);

                gameScopeBuilder.RegisterType<InputManager>()
                       .AsSelf()
                       .SingleInstance()
                       .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);

                gameScopeBuilder.RegisterInstance(GraphicsDevice).AsSelf().SingleInstance();
                gameScopeBuilder.RegisterInstance(_basicEffect).AsSelf().SingleInstance();
                gameScopeBuilder.RegisterInstance(_keyboard).AsSelf().SingleInstance();
                gameScopeBuilder.RegisterInstance(_mouse).AsSelf().SingleInstance();
                gameScopeBuilder.RegisterInstance(this).AsSelf().SingleInstance();
            });

            _systemManager = _gameScope.Resolve<SystemManager>();
            _systemManager.Initialize();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _systemManager.Load();

            base.LoadContent();
        }

        int _frames = 0;
        int _fpsTime = 0;
        protected override void Update(GameTime gameTime)
        {
            _systemManager.Update();

            _fpsTime += gameTime.ElapsedGameTime.Milliseconds;
            if (_fpsTime >= 1000)
            {
                _frames = 0;
                _fpsTime = 0;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            _frames++;

            GraphicsDevice.Clear(Color.Black);

            _systemManager.Draw();

            base.Draw(gameTime);
        }

        protected override void UnloadContent()
        {
            _systemManager.Unload();

            _gameScope.Dispose();

            base.UnloadContent();
        }
    }
}