//Generated code. Manual changes will be clobbered
using Lidgren.Network;
using System;
using System.Collections.Generic;
using FightingGame.ViewModels;

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
            _parsers["Message"] = (lidgrenMessage) =>
            {
                var timeSent = lidgrenMessage.ReadTime(false);
                Action<MessageViewModel> methodExecutor = (message) => _methods.Message(message);
                return _networkManager.ExecuteMethodFromMessage(lidgrenMessage, methodExecutor);
            };
            _parsers["StartInTen"] = (lidgrenMessage) =>
            {
                var timeSent = lidgrenMessage.ReadTime(false);
                Action methodExecutor = () => _methods.StartInTen(timeSent);
                return _networkManager.ExecuteMethodFromMessage(lidgrenMessage, methodExecutor);
            };
            _parsers["NewInput"] = (lidgrenMessage) =>
            {
                var timeSent = lidgrenMessage.ReadTime(false);
                Action<InputState> methodExecutor = (inputState) => _methods.NewInput(inputState);
                return _networkManager.ExecuteMethodFromMessage(lidgrenMessage, methodExecutor);
            };
        }
    }
}