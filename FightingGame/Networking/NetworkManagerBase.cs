using Lidgren.Network;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FightingGame.Networking
{
    public class NetworkManagerBase
    {
        public NetPeer LidgrenPeer { get; private set; }

        public HashSet<RemoteProxy> ConnectedClients { get; private set; }

        public Methods Methods { get; set; }

        private Dictionary<NetConnection, RemoteProxy> _proxyLookup = new Dictionary<NetConnection, RemoteProxy>();

        private IDManager _convoIDManager = new IDManager();

        private Dictionary<long, Action<NetIncomingMessage>> _callbackActions = new Dictionary<long, Action<NetIncomingMessage>>();
        private Dictionary<long, Action<NetIncomingMessage>> _responseSubscriptions = new Dictionary<long, Action<NetIncomingMessage>>();

        private SendOrPostCallback _messageReadyCallback;

        private MessageParser _messageParser;

        public NetworkManagerBase(Methods methods)
        {
            Methods = methods;
            ConnectedClients = new HashSet<RemoteProxy>();
            _messageReadyCallback = new SendOrPostCallback(MessageReady);
            _messageParser = new MessageParser(this, Methods);
        }

       public virtual void ConnectionConnected(RemoteProxy proxy) { }

        public virtual void ConnectionDisconnected() { }

        public byte[] ParseMessage(string commandName, NetIncomingMessage message)
        {
            return _messageParser.ParseMessage(commandName, message);
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
                    var newProxy = new RemoteProxy(this, msg.SenderConnection);
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
                _responseSubscriptions[convoID](msg);
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

        public Task<R> SubscribeToResponse<R>(long conversationID)
        {
            var completionSource = new TaskCompletionSource<R>();
            _responseSubscriptions[conversationID] = (msg) =>
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
            var msg = LidgrenPeer.CreateMessage();
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
            var msg = LidgrenPeer.CreateMessage();
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
            var msg = LidgrenPeer.CreateMessage();
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
            var msg = LidgrenPeer.CreateMessage();
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
            var msg = LidgrenPeer.CreateMessage();
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
            var msg = LidgrenPeer.CreateMessage();
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
            msg.WriteTime(false);
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
