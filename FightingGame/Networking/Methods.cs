using Caliburn.Micro;
using FightingGame.GameLogic;
using FightingGame.GameLogic.Systems;
using FightingGame.ViewModels;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FightingGame.Networking
{
    public class Methods
    {
        private IEventAggregator _eventAggregator;
        private RemoteInputsManager _remoteInputsManager;

        public Methods(IEventAggregator eventAggregator, RemoteInputsManager remoteInputsManager)
        {
            _eventAggregator = eventAggregator;
            _remoteInputsManager = remoteInputsManager;
        }

        public async void Message(MessageViewModel message)
        {
            await _eventAggregator.PublishOnCurrentThreadAsync(message);
        }

        public async void StartInTen(double sendTime)
        {
            await _eventAggregator.PublishOnCurrentThreadAsync(new StartingEvent(sendTime + 10));
        }

        public void NewInput(InputState inputState)
            => _remoteInputsManager.RecievedStates.Enqueue(inputState);
    }
}
