using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FightingGame.ViewModels
{
    public class PageChangeEvent
    {
        public Screen NewPage { get; }

        public PageChangeEvent(Screen newPage)
        {
            NewPage = newPage;
        }
    }

    public class ShellViewModel : Conductor<Screen>, IHandle<PageChangeEvent>
    {
        public ShellViewModel(IEventAggregator eventAggregator, StartPageViewModel startPage)
        {
            eventAggregator.SubscribeOnPublishedThread(this);

            eventAggregator.PublishOnUIThreadAsync(new PageChangeEvent(startPage));
        }

        public Task HandleAsync(PageChangeEvent message, CancellationToken cancellationToken)
        {
            ActivateItem(message.NewPage);
            return Task.CompletedTask;
        }
    }
}
