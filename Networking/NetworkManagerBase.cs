using Lidgren.Network;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Networking
{
    public class NetworkManagerBase<TProxy, TMessageParser> 
        where TProxy : RemoteProxyBase 
        where TMessageParser : MessageParserBase<TProxy>
    {
        public NetPeer LidgrenPeer { get; private set; }

        public HashSet<TProxy> ConnectedClients { get; private set; }

        private Dictionary<NetConnection, TProxy> _proxyLookup = new Dictionary<NetConnection, TProxy>();

        private SendOrPostCallback _messageReadyCallback;

        private TMessageParser _messageParser;

        private Func<NetPeer, NetConnection, TProxy> _proxyFactory;

        public NetworkManagerBase(TMessageParser parser, Func<NetPeer, NetConnection, TProxy> proxyFactory)
        {
            ConnectedClients = new HashSet<TProxy>();

            _messageReadyCallback = new SendOrPostCallback(MessageReady);
            _messageParser = parser;
            _proxyFactory = proxyFactory;
        }

       public virtual void ConnectionConnected(TProxy proxy) { }

        public virtual void ConnectionDisconnected() { }

        public byte[] ParseMessage(string commandName, NetIncomingMessage message)
        {
            return _messageParser.ParseMessage(commandName, message, _proxyLookup[message.SenderConnection]);
        }

        public void Start(NetPeerConfiguration config)
        {
            LidgrenPeer = new NetPeer(config);
            LidgrenPeer.RegisterReceivedCallback(_messageReadyCallback);
            LidgrenPeer.Start();
        }

        public virtual void MessageReady(object _)
        {
            NetIncomingMessage msg = LidgrenPeer.ReadMessage();
            if (msg != null)
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.Data:
                        Parse(msg);
                        break;

                    case NetIncomingMessageType.StatusChanged:
                        HandleStatusChange(msg);
                        break;

                    case NetIncomingMessageType.VerboseDebugMessage:
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.WarningMessage:
                    case NetIncomingMessageType.ErrorMessage:
                        Console.WriteLine(msg.ReadString());
                        break;

                    default:
                        Console.WriteLine("Unhandled type: " + msg.MessageType);
                        break;
                }
                LidgrenPeer.Recycle(msg);
            }
        }

        private void HandleStatusChange(NetIncomingMessage msg)
        {
            var status = (NetConnectionStatus)msg.ReadByte();

            switch (status)
            {
                case NetConnectionStatus.Disconnected:
                    if (_proxyLookup.ContainsKey(msg.SenderConnection))
                    {
                        var connectedProxy = _proxyLookup[msg.SenderConnection];
                        _proxyLookup.Remove(msg.SenderConnection);
                        ConnectedClients.Remove(connectedProxy);
                        ConnectionDisconnected();
                    }
                    break;
                case NetConnectionStatus.Connected:
                    var newProxy = _proxyFactory(LidgrenPeer, msg.SenderConnection);
                    _proxyLookup[msg.SenderConnection] = newProxy;
                    ConnectedClients.Add(newProxy);
                    ConnectionConnected(newProxy);
                    break;
            }
        }

        private void Parse(NetIncomingMessage msg)
        {
            var command = msg.ReadString();

            var convoID = msg.ReadInt64();
            if (command == "response")
            {
                _proxyLookup[msg.SenderConnection].ProcessResponse(msg, convoID);
            }
            else
            {
                var data = ParseMessage(command, msg);
                var responseMessage = LidgrenPeer.CreateMessage();
                responseMessage.Write("response");
                responseMessage.Write(convoID);
                responseMessage.Write(data.Length);
                responseMessage.Write(data);
                LidgrenPeer.SendMessage(responseMessage, msg.SenderConnection, NetDeliveryMethod.ReliableUnordered);
            }
        }

        public void Stop()
        {
            if (LidgrenPeer != null)
            {
                foreach (var connection in LidgrenPeer.Connections)
                {
                    connection.Disconnect("Disconnected by server.");
                }
            }
        }
    }
}
