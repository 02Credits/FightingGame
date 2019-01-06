using Autofac;
using Caliburn.Micro;
using FightingGame.Networking;
using FightingGame.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using INotifyPropertyChanged = System.ComponentModel.INotifyPropertyChanged;

namespace FightingGame
{
    public class Bootstrapper : BootstrapperBase
    {
        private IContainer _container;

        public Bootstrapper()
        {
            Initialize();
        }

        protected override void Configure()
        {
            var builder = new ContainerBuilder();
            var assembly = Assembly.GetExecutingAssembly();

            // register view models
            builder.RegisterAssemblyTypes(AssemblySource.Instance.ToArray())
                // must be a type that ends with ViewModel
                .Where(type => type.Name.EndsWith("ViewModel"))
                // must implement INotifyPropertyChanged (deriving from PropertyChangedBase will satisfy this)
                .Where(type => type.GetInterface(typeof(INotifyPropertyChanged).Name) != null)
                // registered as self
                .AsSelf()
                // always create a new one
                .InstancePerDependency();

            // register views
            builder.RegisterAssemblyTypes(AssemblySource.Instance.ToArray())
                // must be a type that ends with View
                .Where(type => type.Name.EndsWith("View"))
                // registered as self
                .AsSelf()
                // always create a new one
                .InstancePerDependency();

            // register the single window manager for this container
            builder.Register<IWindowManager>(c => new WindowManager()).InstancePerLifetimeScope();
            // register event aggregator
            builder.Register<IEventAggregator>(c => new EventAggregator()).InstancePerLifetimeScope();

            builder.RegisterType<Methods>().AsSelf().SingleInstance();

            _container = builder.Build();
        }

        protected override object GetInstance(Type service, string key) {
            if (_container.IsRegistered(service)) {
                return _container.Resolve(service);
            }
            throw new Exception($"Could not locate any instances of contract {key ?? service.Name}.");
        }

        protected override IEnumerable<object> GetAllInstances(Type service) {
            return _container.Resolve(typeof(IEnumerable<>).MakeGenericType(service)) as IEnumerable<object>;
        }

        protected override void BuildUp(object instance) {
            _container.InjectProperties(instance);
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }
    }
}
