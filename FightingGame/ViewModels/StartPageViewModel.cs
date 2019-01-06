using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FightingGame.ViewModels
{
    public class StartPageViewModel : Screen
    {
        IEventAggregator _eventAggregator;
        Func<ConnectingViewModel> _connectingViewModelFactory;

        public string HostAddress { get; set; }
        public bool CanJoin => !string.IsNullOrWhiteSpace(HostAddress);

        public StartPageViewModel(IEventAggregator eventAggregator, Func<ConnectingViewModel> connectingViewModelFactory)
        {
            _eventAggregator = eventAggregator;
            _connectingViewModelFactory = connectingViewModelFactory;
        }

        public void Join()
        {
            _eventAggregator.PublishOnUIThreadAsync(new PageChangeEvent(_connectingViewModelFactory()));
        }
    }
}
