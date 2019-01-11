using Caliburn.Micro;
using FightingGame.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FightingGame.ViewModels
{
    public interface IPageChangeEvent { }
    public class ResetEvent : IPageChangeEvent { }

    public class ShellViewModel : Conductor<Screen>, IHandle<IPageChangeEvent>
    {
        private HomeViewModel _startPage;
        private ConnectionLostViewModel _connectionLost;
        private Func<bool, LobbyViewModel> _lobbyFactory;
        private Func<bool, ConnectingViewModel> _connectingFactory;
        private Func<double, StartingViewModel> _startingFactory;
        private Func<GameViewModel> _gameFactory;

        public ShellViewModel(
            IEventAggregator eventAggregator, HomeViewModel startPage, ConnectionLostViewModel connectionLost, 
            Func<bool, LobbyViewModel> lobbyFactory, Func<bool, ConnectingViewModel> connectingFactory,
            Func<double, StartingViewModel> startingFactory, Func<GameViewModel> gameFactory)
        {
            _startPage = startPage;
            _connectionLost = connectionLost;
            _lobbyFactory = lobbyFactory;
            _connectingFactory = connectingFactory;
            _startingFactory = startingFactory;
            _gameFactory = gameFactory;

            eventAggregator.SubscribeOnUIThread(this);

            ActivateItem(_startPage);
        }

        public Task HandleAsync(IPageChangeEvent pageChangeEvent, CancellationToken cancellationToken)
        {
            switch (pageChangeEvent)
            {
                case ResetEvent reset:
                    ActivateItem(_startPage);
                    break;
                case ConnectedEvent connected:
                    ActivateItem(_lobbyFactory(connected.IsHosting));
                    break;
                case ConnectionLostEvent connectionLost:
                    ActivateItem(_connectionLost);
                    break;
                case ConnectingEvent connecting:
                    ActivateItem(_connectingFactory(connecting.IsHosting));
                    break;
                case StartingEvent starting:
                    ActivateItem(_startingFactory(starting.StartingTime));
                    break;
                case StartEvent start:
                    ActivateItem(_gameFactory());
                    break;
            }
            return Task.CompletedTask;
        }
    }
}
