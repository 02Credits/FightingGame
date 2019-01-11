using Caliburn.Micro;
using FightingGame.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FightingGame.ViewModels
{
    public class ConnectingEvent : IPageChangeEvent
    {
        public bool IsHosting { get; }
        public ConnectingEvent(bool isHosting) => IsHosting = isHosting;
    }

    public class HomeViewModel : Screen
    {
        IEventAggregator _eventAggregator;
        NetworkManager _networkManager;

        public string HostAddress { get; set; }
        public bool CanJoin => !string.IsNullOrWhiteSpace(HostAddress);

        public HomeViewModel(IEventAggregator eventAggregator, NetworkManager networkManager)
        {
            _eventAggregator = eventAggregator;
            _networkManager = networkManager;
        }

        public async void Join()
        {
            _networkManager.Connect(HostAddress);
            await _eventAggregator.PublishOnUIThreadAsync(new ConnectingEvent(false));
        }

        public async void Host()
        {
            _networkManager.Host();
            await _eventAggregator.PublishOnUIThreadAsync(new ConnectingEvent(true));
        }
    }
}
