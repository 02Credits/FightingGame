using Caliburn.Micro;
using FightingGame.Networking;
using FightingGame.Views;
using Lidgren.Network;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FightingGame.ViewModels
{
    public class LobbyViewModel : Screen, IHandle<MessageViewModel>
    {
        public bool Hosting { get; private set; }
        public ObservableCollection<MessageViewModel> Messages { get; }
        public string Input { get; set; }

        private IEventAggregator _eventAggregator;

        public LobbyViewModel(IEventAggregator eventAggregator, bool hosting)
        {
            Hosting = hosting;
            Messages = new ObservableCollection<MessageViewModel>();

            _eventAggregator = eventAggregator;
            _eventAggregator.SubscribeOnUIThread(this);
        }

        public async void Send()
        {
            var message = new MessageViewModel(Input);
            Input = "";

            Messages.Add(message);
            await _eventAggregator.PublishOnCurrentThreadAsync(new BroadcastEvent(remote => remote.Message(message)));
        }

        public async void Start()
        {
            await _eventAggregator.PublishOnCurrentThreadAsync(new BroadcastEvent(remote => remote.StartInTen()));
            await _eventAggregator.PublishOnCurrentThreadAsync(new StartingEvent(NetTime.Now + 10));
        }

        public Task HandleAsync(MessageViewModel message, CancellationToken cancellationToken)
        {
            Messages.Add(message);
            return Task.CompletedTask;
        }
    }
}
