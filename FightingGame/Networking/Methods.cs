using FightingGame.GameLogic;
using FightingGame.GameLogic.Systems;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FightingGame.Networking
{
    public class Methods
    {
        private RemoteInputsManager _remoteInputsManager;

        public Methods(RemoteInputsManager remoteInputsManager)
        {
            _remoteInputsManager = remoteInputsManager;
        }

        public void NewInput(InputState inputState)
            => _remoteInputsManager.RecievedStates.Enqueue(inputState);
    }
}
