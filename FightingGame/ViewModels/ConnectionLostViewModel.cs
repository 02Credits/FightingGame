using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FightingGame.ViewModels
{
    public class ConnectionLostViewModel : Screen
    {
        IEventAggregator _eventAggregator;

        public ConnectionLostViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        public async void StartPage()
        {
            await _eventAggregator.PublishOnCurrentThreadAsync(new ResetEvent());
        }
    }
}
