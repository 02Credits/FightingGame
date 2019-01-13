//Generated code. Manual changes will be clobbered
using Lidgren.Network;
using System;
using System.Collections.Generic;
using FightingGame.ViewModels;
using Networking;

namespace FightingGame.Networking
{
    public class MessageParser : MessageParserBase<RemoteProxy>
    {
        private Methods _methods;
        private Dictionary<string, Func<NetIncomingMessage, RemoteProxy, byte[]>> _parsers;

        public MessageParser(Methods methods)
        {
            _methods = methods;
            _parsers = new Dictionary<string, Func<NetIncomingMessage, RemoteProxy, byte[]>>();
            PopulateParsers();
        }

        public override byte[] ParseMessage(string command, NetIncomingMessage msg, RemoteProxy proxy)
        {
            return _parsers[command](msg, proxy);
        }

        private void PopulateParsers()
        {
            _parsers["Message"] = (lidgrenMessage, proxy) =>
            {
                var timeSent = lidgrenMessage.ReadTime(false);
                Action<MessageViewModel> methodExecutor = (message) => _methods.Message(message);
                return ExecuteMethodFromMessage(lidgrenMessage, methodExecutor);
            };
            _parsers["StartInTen"] = (lidgrenMessage, proxy) =>
            {
                var timeSent = lidgrenMessage.ReadTime(false);
                Action methodExecutor = () => _methods.StartInTen(timeSent);
                return ExecuteMethodFromMessage(lidgrenMessage, methodExecutor);
            };
            _parsers["NewInput"] = (lidgrenMessage, proxy) =>
            {
                var timeSent = lidgrenMessage.ReadTime(false);
                Action<InputState> methodExecutor = (inputState) => _methods.NewInput(inputState);
                return ExecuteMethodFromMessage(lidgrenMessage, methodExecutor);
            };
        }
    }
}