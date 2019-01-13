using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Networking
{
    public abstract class MessageParserBase<TProxy> where TProxy : RemoteProxyBase
    {
        public abstract byte[] ParseMessage(string command, NetIncomingMessage msg, TProxy remoteProxy);

        protected byte[] ExecuteMethodFromMessage(NetIncomingMessage message, Action method)
        {
            method();
            return new byte[] { };
        }

        protected byte[] ExecuteMethodFromMessage<R>(NetIncomingMessage message, Func<R> method)
        {
            var returnVal = method();
            return SerializationUtils.Serialize(returnVal);
        }

        protected T ParseParameter<T>(NetIncomingMessage message)
        {
            var paramDataLength = message.ReadInt32();
            var paramData = message.ReadBytes(paramDataLength);
            return SerializationUtils.Deserialize<T>(paramData);
        }

        protected byte[] ExecuteMethodFromMessage<T1>(NetIncomingMessage message, Action<T1> method)
        {
            var param1 = ParseParameter<T1>(message);

            method(param1);
            return new byte[] { };
        }

        protected byte[] ExecuteMethodFromMessage<T1, R>(NetIncomingMessage message, Func<T1, R> method)
        {
            var param1 = ParseParameter<T1>(message);

            var returnVal = method(param1);
            return SerializationUtils.Serialize(returnVal);
        }

        protected byte[] ExecuteMethodFromMessage<T1, T2>(NetIncomingMessage message, Action<T1, T2> method)
        {
            var param1 = ParseParameter<T1>(message);
            var param2 = ParseParameter<T2>(message);

            method(param1, param2);
            return new byte[] { };
        }

        protected byte[] ExecuteMethodFromMessage<T1, T2, R>(NetIncomingMessage message, Func<T1, T2, R> method)
        {
            var param1 = ParseParameter<T1>(message);
            var param2 = ParseParameter<T2>(message);

            var returnVal = method(param1, param2);
            return SerializationUtils.Serialize(returnVal);
        }

        protected byte[] ExecuteMethodFromMessage<T1, T2, T3>(NetIncomingMessage message, Action<T1, T2, T3> method)
        {
            var param1 = ParseParameter<T1>(message);
            var param2 = ParseParameter<T2>(message);
            var param3 = ParseParameter<T3>(message);

            method(param1, param2, param3);
            return new byte[] { };
        }

        protected byte[] ExecuteMethodFromMessage<T1, T2, T3, R>(NetIncomingMessage message, Func<T1, T2, T3, R> method)
        {
            var param1 = ParseParameter<T1>(message);
            var param2 = ParseParameter<T2>(message);
            var param3 = ParseParameter<T3>(message);

            var returnVal = method(param1, param2, param3);
            return SerializationUtils.Serialize(returnVal);
        }

        protected byte[] ExecuteMethodFromMessage<T1, T2, T3, T4>(NetIncomingMessage message, Action<T1, T2, T3, T4> method)
        {
            var param1 = ParseParameter<T1>(message);
            var param2 = ParseParameter<T2>(message);
            var param3 = ParseParameter<T3>(message);
            var param4 = ParseParameter<T4>(message);

            method(param1, param2, param3, param4);
            return new byte[] { };
        }

        protected byte[] ExecuteMethodFromMessage<T1, T2, T3, T4, R>(NetIncomingMessage message, Func<T1, T2, T3, T4, R> method)
        {
            var param1 = ParseParameter<T1>(message);
            var param2 = ParseParameter<T2>(message);
            var param3 = ParseParameter<T3>(message);
            var param4 = ParseParameter<T4>(message);

            var returnVal = method(param1, param2, param3, param4);
            return SerializationUtils.Serialize(returnVal);
        }

        protected byte[] ExecuteMethodFromMessage<T1, T2, T3, T4, T5>(NetIncomingMessage message, Action<T1, T2, T3, T4, T5> method)
        {
            var param1 = ParseParameter<T1>(message);
            var param2 = ParseParameter<T2>(message);
            var param3 = ParseParameter<T3>(message);
            var param4 = ParseParameter<T4>(message);
            var param5 = ParseParameter<T5>(message);

            method(param1, param2, param3, param4, param5);
            return new byte[] { };
        }

        protected byte[] ExecuteMethodFromMessage<T1, T2, T3, T4, T5, R>(NetIncomingMessage message, Func<T1, T2, T3, T4, T5, R> method)
        {
            var param1 = ParseParameter<T1>(message);
            var param2 = ParseParameter<T2>(message);
            var param3 = ParseParameter<T3>(message);
            var param4 = ParseParameter<T4>(message);
            var param5 = ParseParameter<T5>(message);

            var returnVal = method(param1, param2, param3, param4, param5);
            return SerializationUtils.Serialize(returnVal);
        }
    }
}
