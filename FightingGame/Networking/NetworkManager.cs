using Caliburn.Micro;
using FightingGame.ViewModels;
using Lidgren.Network;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FightingGame.Networking
{
    public class ConnectionLostEvent : IPageChangeEvent { }

    public class ConnectedEvent : IPageChangeEvent
    {
        public bool IsHosting { get; }
        public ConnectedEvent(bool isHosting) => IsHosting = isHosting;
    }

    public class BroadcastEvent
    {
        public Func<RemoteProxy, Task> Execute { get; }
        public BroadcastEvent(Func<RemoteProxy, Task> execute) => Execute = execute;
    }

    public class NetworkManager : NetworkManagerBase, IHandle<BroadcastEvent> 
    {
        public const int PORT = 8080;

        IEventAggregator _eventAggregator;

        public bool Hosting { get; private set; }

        public NetworkManager(Methods methods, IEventAggregator eventAggregator) 
            : base(methods)
        {
            _eventAggregator = eventAggregator;

            _eventAggregator.SubscribeOnUIThread(this);
        }

        public void Connect(string host)
        {
            var config = new NetPeerConfiguration("fighting-game");
            Start(config);
            if (host.ToLower() == "localhost") host = "127.0.0.1";
            LidgrenPeer.Connect(host, PORT);
            Hosting = false;
        }

        public void Host()
        {
            var config = new NetPeerConfiguration("fighting-game");
            config.Port = PORT;
            config.AcceptIncomingConnections = true;
            Start(config);
            Hosting = true;
        }

        public async override void ConnectionConnected(RemoteProxy proxy)
        {
            await _eventAggregator.PublishOnCurrentThreadAsync(new ConnectedEvent(Hosting));
        }

        public async override void ConnectionDisconnected()
        {
            await _eventAggregator.PublishOnCurrentThreadAsync(new ConnectionLostEvent());
            LidgrenPeer.Shutdown("Connection lost...");
        }

        public async Task HandleAsync(BroadcastEvent message, CancellationToken cancellationToken)
        {
            foreach (RemoteProxy remote in ConnectedClients)
            {
                await message.Execute(remote);
            }
        }
    }
}