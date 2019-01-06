//Generated code. Manual changes will be clobbered
using FightingGame.GameLogic;
using Lidgren.Network;
using System;
using System.Collections.Generic;

namespace FightingGame.Networking
{
    public class MessageParser
    {
        private NetworkManagerBase _networkManager;
        private Methods _methods;
        private Dictionary<string, Func<NetIncomingMessage, byte[]>> _parsers;

        public MessageParser(NetworkManagerBase networkManager, Methods methods)
        {
            _networkManager = networkManager;
            _methods = methods;
            _parsers = new Dictionary<string, Func<NetIncomingMessage, byte[]>>();
            PopulateParsers();
        }

        public byte[] ParseMessage(string command, NetIncomingMessage msg)
        {
            return _parsers[command](msg);
        }

        private void PopulateParsers()
        {
            _parsers["NewInput"] = (lidgrenMessage) =>
            {
                Action<InputState> methodExecutor = (inputState) => _methods.NewInput(inputState);
                return _networkManager.ExecuteMethodFromMessage(lidgrenMessage, methodExecutor);
            };
        }
    }
}