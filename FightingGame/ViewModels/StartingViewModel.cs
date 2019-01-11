using Caliburn.Micro;
using FightingGame.Networking;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace FightingGame.ViewModels
{
    public class StartingEvent: IPageChangeEvent
    {
        public double StartingTime { get; }
        public StartingEvent(double startingTime) => StartingTime = startingTime;
    }

    public class StartingViewModel : Screen
    {
        IEventAggregator _eventAggregator;
        double _startTime;
        DispatcherTimer _timer;

        public double TimeTillStart { get; private set; }
        public string TimerText => ((int)TimeTillStart).ToString();

        public StartingViewModel(IEventAggregator eventAggregator, double startTime)
        {
            _eventAggregator = eventAggregator;
            _startTime = startTime;
            _timer = new DispatcherTimer();
            _timer.Tick += Tick;
            _timer.Interval = new TimeSpan(0,0,0,0,1);
            _timer.Start();
        }

        public async void Tick(object _, EventArgs __)
        {
            if (NetTime.Now > _startTime)
            {
                _timer.Stop();
                await _eventAggregator.PublishOnCurrentThreadAsync(new StartEvent());
            }
            else
            {
                TimeTillStart = _startTime - NetTime.Now;
            }
        }

        protected override void OnDeactivate(bool close)
        {
            if (_timer?.IsEnabled ?? false)
            {
                _timer.Stop();
            }
            base.OnDeactivate(close);
        }
    }
}
