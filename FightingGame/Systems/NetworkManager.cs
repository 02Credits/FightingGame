using FightingGame.Systems.Interfaces;
using Lidgren.Network;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FightingGame.Networking;

namespace FightingGame.Systems
{
    public class NetworkManager : IUpdatedSystem, IUnloadedSystem
    {
        public NetPeer LidgrenPeer { get; private set; }

        public HashSet<RemoteProxy> ConnectedClients { get; private set; }

        private Dictionary<NetConnection, RemoteProxy> _proxyLookup = new Dictionary<NetConnection, RemoteProxy>();

        private IDManager _ackIDManager = new IDManager();
        private IDManager _convoIDManager = new IDManager();

        private Dictionary<long, Tuple<DateTime, NetConnection, NetOutgoingMessage>> _nonAckedMessages = new Dictionary<long, Tuple<DateTime, NetConnection, NetOutgoingMessage>>();
        private Dictionary<long, Action<NetIncomingMessage>> _callbackActions = new Dictionary<long, Action<NetIncomingMessage>>();

        private ConcurrentQueue<NetIncomingMessage> _messages = new ConcurrentQueue<NetIncomingMessage>();

        private Dictionary<NetConnection, List<long>> _seenAckIDs = new Dictionary<NetConnection, List<long>>();
        private Dictionary<long, Action<NetIncomingMessage>> _conversationSubscriptions = new Dictionary<long, Action<NetIncomingMessage>>();

        private MessageParser _messageParser;

        public void Connect(string host, int port)
        {
            var config = new NetPeerConfiguration("fighting-game");
            Start(config);
            LidgrenPeer.Connect(host, port);
        }

        public void Host()
        {
            var config = new NetPeerConfiguration("fighting-game");
            config.Port = 8080;
            config.AcceptIncomingConnections = true;
            Start(config);
        }

        public NetworkManager()
        {
            _messageParser = new MessageParser(this);
            ConnectedClients = new HashSet<RemoteProxy>();
        }

        public void ConnectionConnected(NetConnection connection)
        {
            var proxy = new RemoteProxy(this, connection);
            _proxyLookup[connection] = proxy;
            ConnectedClients.Add(proxy);
            Game.Play();
        }

        public void ConnectionDisconnected(NetConnection connection)
        {
            if (_proxyLookup.ContainsKey(connection))
            {
                var proxy = _proxyLookup[connection];
                _proxyLookup.Remove(connection);
                ConnectedClients.Remove(proxy);

                if (ConnectedClients.Any())
                {
                    Game.Play();
                }
                else
                {
                    LidgrenPeer.Shutdown("Nobody connected :(");
                    Game.Start();
                }
            }
        }

        public byte[] ParseMessage(string commandName, NetIncomingMessage message)
        {
            return _messageParser.ParseMessage(commandName, message);
        }

        public void Start(NetPeerConfiguration config)
        {
            LidgrenPeer = new NetPeer(config);
            LidgrenPeer.Start();
            Task.Run(() =>
            {
                while (true)
                {
                    if (LidgrenPeer.Status == NetPeerStatus.Running)
                    {
                        NetIncomingMessage msg = LidgrenPeer.ReadMessage();
                        if (msg != null)
                        {
                            _messages.Enqueue(msg);
                            continue;
                        }
                    }
                    Thread.Sleep(16);
                }
            });
        }

        public virtual void Update()
        {
            if (Game.Rewinding) return;

            NetIncomingMessage msg;
            while (_messages.TryDequeue(out msg))
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.Data:
                        var ackID = msg.ReadInt64();
                        ParseOrHandleAck(msg, ackID);
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

            ResendMessages();
        }

        private void ResendMessages()
        {
            foreach (var id in _nonAckedMessages.Keys.ToList())
            {
                var data = _nonAckedMessages[id];
                var sentTime = data.Item1;
                var recipient = data.Item2;
                var nonAckedMessage = data.Item3;

                if ((DateTime.Now - sentTime).TotalSeconds > 0.1)
                {
                    if (nonAckedMessage.LengthBits != 0)
                    {
                        var ackMessage = LidgrenPeer.CreateMessage();
                        ackMessage.Write(nonAckedMessage.PeekDataBuffer());
                        LidgrenPeer.SendMessage(ackMessage, recipient, NetDeliveryMethod.Unreliable);
                        _nonAckedMessages[id] = Tuple.Create(DateTime.Now, recipient, nonAckedMessage);
                    }
                    else
                    {
                        _nonAckedMessages.Remove(id);
                    }
                }
            }
        }

        private void HandleStatusChange(NetIncomingMessage msg)
        {
            var status = (NetConnectionStatus)msg.ReadByte();
            switch (status)
            {
                case NetConnectionStatus.Disconnected:
                    ConnectionDisconnected(msg.SenderConnection);
                    break;
                case NetConnectionStatus.Connected:
                    ConnectionConnected(msg.SenderConnection);
                    break;
            }
        }

        private void ParseOrHandleAck(NetIncomingMessage msg, long ackID)
        {
            var command = msg.ReadString();
            if (command == "ack")
            {
                _nonAckedMessages.Remove(ackID);
            }
            else
            {
                var ackMessage = LidgrenPeer.CreateMessage();
                ackMessage.Write(ackID);
                ackMessage.Write("ack");
                LidgrenPeer.SendMessage(ackMessage, msg.SenderConnection, NetDeliveryMethod.Unreliable);
                if (!_seenAckIDs.ContainsKey(msg.SenderConnection))
                {
                    _seenAckIDs[msg.SenderConnection] = new List<long>();
                }

                if (!_seenAckIDs[msg.SenderConnection].Contains(ackID))
                {
                    _seenAckIDs[msg.SenderConnection].Add(ackID);
                    var convoID = msg.ReadInt64();
                    if (command == "response")
                    {
                        _conversationSubscriptions[convoID](msg);
                    }
                    else
                    {
                        var data = ParseMessage(command, msg);
                        var responseMessage = GetAckedMessage(msg.SenderConnection);
                        responseMessage.Write("response");
                        responseMessage.Write(convoID);
                        responseMessage.Write(data.Length);
                        responseMessage.Write(data);
                        LidgrenPeer.SendMessage(responseMessage, msg.SenderConnection, NetDeliveryMethod.Unreliable);
                    }
                }
            }
        }

        public NetOutgoingMessage GetAckedMessage(NetConnection target)
        {
            var msg = LidgrenPeer.CreateMessage();
            var ackID = _ackIDManager.GetNextID();
            msg.Write(ackID);
            _nonAckedMessages[ackID] = Tuple.Create(DateTime.Now, target, msg);
            return msg;
        }

        public Task<R> SubscribeToResponse<R>(long conversationID)
        {
            var completionSource = new TaskCompletionSource<R>();
            _conversationSubscriptions[conversationID] = (msg) =>
            {
                var dataLength = msg.ReadInt32();
                if (dataLength > 0)
                {
                    var data = msg.ReadBytes(dataLength);
                    var obj = SerializationUtils.Deserialize<R>(data);
                    completionSource.SetResult(obj);
                }
                else
                {
                    completionSource.SetResult(default(R));
                }
            };
            return completionSource.Task;
        }

        public Task<R> SendCommand<R>(string commandName)
        {
            return SendCommand<R>(LidgrenPeer.Connections[0], commandName);
        }

        public Task<R> SendCommand<R>(NetConnection connection, string commandName)
        {
            var msg = GetAckedMessage(connection);
            var id = WriteCommand(msg, commandName);
            LidgrenPeer.SendMessage(msg, connection, NetDeliveryMethod.Unreliable);
            return SubscribeToResponse<R>(id);
        }

        public Task<R> SendCommand<R, T1>(string commandName, T1 param1)
        {
            return SendCommand<R, T1>(LidgrenPeer.Connections[0], commandName, param1);
        }

        public Task<R> SendCommand<R, T1>(NetConnection connection, string commandName, T1 param1)
        {
            var msg = GetAckedMessage(connection);
            var id = WriteCommand(msg, commandName, param1);
            LidgrenPeer.SendMessage(msg, connection, NetDeliveryMethod.Unreliable);
            return SubscribeToResponse<R>(id);
        }

        public Task<R> SendCommand<R, T1, T2>(string commandName, T1 param1, T2 param2)
        {
            return SendCommand<R, T1, T2>(LidgrenPeer.Connections[0], commandName, param1, param2);
        }

        public Task<R> SendCommand<R, T1, T2>(NetConnection connection, string commandName, T1 param1, T2 param2)
        {
            var msg = GetAckedMessage(connection);
            var id = WriteCommand(msg, commandName, param1, param2);
            LidgrenPeer.SendMessage(msg, connection, NetDeliveryMethod.Unreliable);
            return SubscribeToResponse<R>(id);
        }

        public Task<R> SendCommand<R, T1, T2, T3>(string commandName, T1 param1, T2 param2, T3 param3)
        {
            return SendCommand<R, T1, T2, T3>(LidgrenPeer.Connections[0], commandName, param1, param2, param3);
        }

        public Task<R> SendCommand<R, T1, T2, T3>(NetConnection connection, string commandName, T1 param1, T2 param2, T3 param3)
        {
            var msg = GetAckedMessage(connection);
            var id = WriteCommand(msg, commandName, param1, param2, param3);
            LidgrenPeer.SendMessage(msg, connection, NetDeliveryMethod.Unreliable);
            return SubscribeToResponse<R>(id);
        }

        public Task<R> SendCommand<R, T1, T2, T3, T4>(string commandName, T1 param1, T2 param2, T3 param3, T4 param4)
        {
            return SendCommand<R, T1, T2, T3, T4>(LidgrenPeer.Connections[0], commandName, param1, param2, param3, param4);
        }

        public Task<R> SendCommand<R, T1, T2, T3, T4>(NetConnection connection, string commandName, T1 param1, T2 param2, T3 param3, T4 param4)
        {
            var msg = GetAckedMessage(connection);
            var id = WriteCommand(msg, commandName, param1, param2, param3, param4);
            LidgrenPeer.SendMessage(msg, connection, NetDeliveryMethod.Unreliable);
            return SubscribeToResponse<R>(id);
        }

        public Task<R> SendCommand<R, T1, T2, T3, T4, T5>(string commandName, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5)
        {
            return SendCommand<R, T1, T2, T3, T4, T5>(LidgrenPeer.Connections[0], commandName, param1, param2, param3, param4, param5);
        }

        public Task<R> SendCommand<R, T1, T2, T3, T4, T5>(NetConnection connection, string commandName, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5)
        {
            var msg = GetAckedMessage(connection);
            var id = WriteCommand(msg, commandName, param1, param2, param3, param4, param5);
            LidgrenPeer.SendMessage(msg, connection, NetDeliveryMethod.Unreliable);
            return SubscribeToResponse<R>(id);
        }

        public void WriteMessageParam<T1>(NetOutgoingMessage msg, T1 param)
        {
            var data = SerializationUtils.Serialize(param);
            msg.Write(data.Length);
            msg.Write(data);
        }

        public long WriteCommand(NetOutgoingMessage msg, string commandName)
        {
            msg.Write(commandName);
            var convoID = _convoIDManager.GetNextID();
            msg.Write(convoID);
            return convoID;
        }

        public long WriteCommand<T1>(NetOutgoingMessage msg, string commandName, T1 param1)
        {
            var id = WriteCommand(msg, commandName);
            WriteMessageParam(msg, param1);
            return id;
        }

        public long WriteCommand<T1, T2>(NetOutgoingMessage msg, string commandName, T1 param1, T2 param2)
        {
            var id = WriteCommand(msg, commandName, param1);
            WriteMessageParam(msg, param2);
            return id;
        }

        public long WriteCommand<T1, T2, T3>(NetOutgoingMessage msg, string commandName, T1 param1, T2 param2, T3 param3)
        {
            var id = WriteCommand(msg, commandName, param1, param2);
            WriteMessageParam(msg, param3);
            return id;
        }

        public long WriteCommand<T1, T2, T3, T4>(NetOutgoingMessage msg, string commandName, T1 param1, T2 param2, T3 param3, T4 param4)
        {
            var id = WriteCommand(msg, commandName, param1, param2, param3);
            WriteMessageParam(msg, param4);
            return id;
        }

        public long WriteCommand<T1, T2, T3, T4, T5>(NetOutgoingMessage msg, string commandName, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5)
        {
            var id = WriteCommand(msg, commandName, param1, param2, param3, param4);
            WriteMessageParam(msg, param5);
            return id;
        }

        public byte[] ExecuteMethodFromMessage(NetIncomingMessage message, Action method)
        {
            method();
            return new byte[] { };
        }

        public byte[] ExecuteMethodFromMessage<R>(NetIncomingMessage message, Func<R> method)
        {
            var returnVal = method();
            return SerializationUtils.Serialize(returnVal);
        }

        public T ParseParameter<T>(NetIncomingMessage message)
        {
            var paramDataLength = message.ReadInt32();
            var paramData = message.ReadBytes(paramDataLength);
            return SerializationUtils.Deserialize<T>(paramData);
        }

        public byte[] ExecuteMethodFromMessage<T1>(NetIncomingMessage message, Action<T1> method)
        {
            var param1 = ParseParameter<T1>(message);

            method(param1);
            return new byte[] { };
        }

        public byte[] ExecuteMethodFromMessage<T1, R>(NetIncomingMessage message, Func<T1, R> method)
        {
            var param1 = ParseParameter<T1>(message);

            var returnVal = method(param1);
            return SerializationUtils.Serialize(returnVal);
        }

        public byte[] ExecuteMethodFromMessage<T1, T2>(NetIncomingMessage message, Action<T1, T2> method)
        {
            var param1 = ParseParameter<T1>(message);
            var param2 = ParseParameter<T2>(message);

            method(param1, param2);
            return new byte[] { };
        }

        public byte[] ExecuteMethodFromMessage<T1, T2, R>(NetIncomingMessage message, Func<T1, T2, R> method)
        {
            var param1 = ParseParameter<T1>(message);
            var param2 = ParseParameter<T2>(message);

            var returnVal = method(param1, param2);
            return SerializationUtils.Serialize(returnVal);
        }

        public byte[] ExecuteMethodFromMessage<T1, T2, T3>(NetIncomingMessage message, Action<T1, T2, T3> method)
        {
            var param1 = ParseParameter<T1>(message);
            var param2 = ParseParameter<T2>(message);
            var param3 = ParseParameter<T3>(message);

            method(param1, param2, param3);
            return new byte[] { };
        }

        public byte[] ExecuteMethodFromMessage<T1, T2, T3, R>(NetIncomingMessage message, Func<T1, T2, T3, R> method)
        {
            var param1 = ParseParameter<T1>(message);
            var param2 = ParseParameter<T2>(message);
            var param3 = ParseParameter<T3>(message);

            var returnVal = method(param1, param2, param3);
            return SerializationUtils.Serialize(returnVal);
        }

        public byte[] ExecuteMethodFromMessage<T1, T2, T3, T4>(NetIncomingMessage message, Action<T1, T2, T3, T4> method)
        {
            var param1 = ParseParameter<T1>(message);
            var param2 = ParseParameter<T2>(message);
            var param3 = ParseParameter<T3>(message);
            var param4 = ParseParameter<T4>(message);

            method(param1, param2, param3, param4);
            return new byte[] { };
        }

        public byte[] ExecuteMethodFromMessage<T1, T2, T3, T4, R>(NetIncomingMessage message, Func<T1, T2, T3, T4, R> method)
        {
            var param1 = ParseParameter<T1>(message);
            var param2 = ParseParameter<T2>(message);
            var param3 = ParseParameter<T3>(message);
            var param4 = ParseParameter<T4>(message);

            var returnVal = method(param1, param2, param3, param4);
            return SerializationUtils.Serialize(returnVal);
        }

        public byte[] ExecuteMethodFromMessage<T1, T2, T3, T4, T5>(NetIncomingMessage message, Action<T1, T2, T3, T4, T5> method)
        {
            var param1 = ParseParameter<T1>(message);
            var param2 = ParseParameter<T2>(message);
            var param3 = ParseParameter<T3>(message);
            var param4 = ParseParameter<T4>(message);
            var param5 = ParseParameter<T5>(message);

            method(param1, param2, param3, param4, param5);
            return new byte[] { };
        }

        public byte[] ExecuteMethodFromMessage<T1, T2, T3, T4, T5, R>(NetIncomingMessage message, Func<T1, T2, T3, T4, T5, R> method)
        {
            var param1 = ParseParameter<T1>(message);
            var param2 = ParseParameter<T2>(message);
            var param3 = ParseParameter<T3>(message);
            var param4 = ParseParameter<T4>(message);
            var param5 = ParseParameter<T5>(message);

            var returnVal = method(param1, param2, param3, param4, param5);
            return SerializationUtils.Serialize(returnVal);
        }

        public void Unload()
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