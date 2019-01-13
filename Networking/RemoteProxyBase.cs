using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace Networking
{
    public abstract class RemoteProxyBase
    {
        NetPeer _peer;
        NetConnection _connection;

        IDManager _convoIDManager = new IDManager();
        Dictionary<long, Action<NetIncomingMessage>> _responseSubscriptions = new Dictionary<long, Action<NetIncomingMessage>>();

        public RemoteProxyBase(NetPeer peer, NetConnection connection)
        {
            _peer = peer;
            _connection = connection;
        }

        public void ProcessResponse(NetIncomingMessage message, long convoID)
        {
            _responseSubscriptions[convoID](message);
        }

        protected Task<R> SubscribeToResponse<R>(long conversationID)
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

        protected Task<R> SendCommand<R>(string commandName)
        {
            var msg = _peer.CreateMessage();
            var id = WriteCommand(msg, commandName);
            _peer.SendMessage(msg, _connection, NetDeliveryMethod.Unreliable);
            return SubscribeToResponse<R>(id);
        }

        protected Task<R> SendCommand<R, T1>(string commandName, T1 param1)
        {
            var msg = _peer.CreateMessage();
            var id = WriteCommand(msg, commandName, param1);
            _peer.SendMessage(msg, _connection, NetDeliveryMethod.Unreliable);
            return SubscribeToResponse<R>(id);
        }

        protected Task<R> SendCommand<R, T1, T2>(string commandName, T1 param1, T2 param2)
        {
            var msg = _peer.CreateMessage();
            var id = WriteCommand(msg, commandName, param1, param2);
            _peer.SendMessage(msg, _connection, NetDeliveryMethod.Unreliable);
            return SubscribeToResponse<R>(id);
        }

        protected Task<R> SendCommand<R, T1, T2, T3>(string commandName, T1 param1, T2 param2, T3 param3)
        {
            var msg = _peer.CreateMessage();
            var id = WriteCommand(msg, commandName, param1, param2, param3);
            _peer.SendMessage(msg, _connection, NetDeliveryMethod.Unreliable);
            return SubscribeToResponse<R>(id);
        }

        protected Task<R> SendCommand<R, T1, T2, T3, T4>(string commandName, T1 param1, T2 param2, T3 param3, T4 param4)
        {
            var msg = _peer.CreateMessage();
            var id = WriteCommand(msg, commandName, param1, param2, param3, param4);
            _peer.SendMessage(msg, _connection, NetDeliveryMethod.Unreliable);
            return SubscribeToResponse<R>(id);
        }

        protected Task<R> SendCommand<R, T1, T2, T3, T4, T5>(string commandName, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5)
        {
            var msg = _peer.CreateMessage();
            var id = WriteCommand(msg, commandName, param1, param2, param3, param4, param5);
            _peer.SendMessage(msg, _connection, NetDeliveryMethod.Unreliable);
            return SubscribeToResponse<R>(id);
        }

        protected void WriteMessageParam<T1>(NetOutgoingMessage msg, T1 param)
        {
            var data = SerializationUtils.Serialize(param);
            msg.Write(data.Length);
            msg.Write(data);
        }

        protected long WriteCommand(NetOutgoingMessage msg, string commandName)
        {
            msg.Write(commandName);
            var convoID = _convoIDManager.GetNextID();
            msg.Write(convoID);
            msg.WriteTime(false);
            return convoID;
        }

        protected long WriteCommand<T1>(NetOutgoingMessage msg, string commandName, T1 param1)
        {
            var id = WriteCommand(msg, commandName);
            WriteMessageParam(msg, param1);
            return id;
        }

        protected long WriteCommand<T1, T2>(NetOutgoingMessage msg, string commandName, T1 param1, T2 param2)
        {
            var id = WriteCommand(msg, commandName, param1);
            WriteMessageParam(msg, param2);
            return id;
        }

        protected long WriteCommand<T1, T2, T3>(NetOutgoingMessage msg, string commandName, T1 param1, T2 param2, T3 param3)
        {
            var id = WriteCommand(msg, commandName, param1, param2);
            WriteMessageParam(msg, param3);
            return id;
        }

        protected long WriteCommand<T1, T2, T3, T4>(NetOutgoingMessage msg, string commandName, T1 param1, T2 param2, T3 param3, T4 param4)
        {
            var id = WriteCommand(msg, commandName, param1, param2, param3);
            WriteMessageParam(msg, param4);
            return id;
        }

        protected long WriteCommand<T1, T2, T3, T4, T5>(NetOutgoingMessage msg, string commandName, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5)
        {
            var id = WriteCommand(msg, commandName, param1, param2, param3, param4);
            WriteMessageParam(msg, param5);
            return id;
        }
    }
}
