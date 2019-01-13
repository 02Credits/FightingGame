//Generated code. Manual changes will be clobbered
using Lidgren.Network;
using System.Threading.Tasks;
using Networking;
using FightingGame.ViewModels;

namespace FightingGame.Networking
{
    public class RemoteProxy : RemoteProxyBase
    {        
        public RemoteProxy(NetPeer peer, NetConnection networkConnection)
            : base(peer, networkConnection) { }

        public Task<object> Message(MessageViewModel message)
        {
            return SendCommand<object, MessageViewModel>("Message", message);
        }

        public Task<object> StartInTen()
        {
            return SendCommand<object>("StartInTen");
        }

        public Task<object> NewInput(InputState inputState)
        {
            return SendCommand<object, InputState>("NewInput", inputState);
        }
    }
}