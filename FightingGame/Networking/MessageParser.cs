//Generated code. Manual changes will be clobbered
using FightingGame.Systems;
using Lidgren.Network;
using System;
using System.Collections.Generic;

namespace FightingGame.Networking
{
    public class MessageParser
    {
        private NetworkManager _networkManager;
        private Dictionary<string, Func<NetIncomingMessage, byte[]>> _parsers;

        public MessageParser(NetworkManager networkManager)
        {
            _networkManager = networkManager;
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
                Action<string, int, InputState> methodExecutor = (playerId, inputFrame, inputState) => Methods.NewInput(playerId, inputFrame, inputState);
                return _networkManager.ExecuteMethodFromMessage(lidgrenMessage, methodExecutor);
            };
        }
    }
}