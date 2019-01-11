//Generated code. Manual changes will be clobbered
using Lidgren.Network;
using System.Threading.Tasks;
using FightingGame.ViewModels;

namespace FightingGame.Networking
{
    public class RemoteProxy
    {        
        NetworkManagerBase _networkManager;
        NetConnection _networkConnection;

        public RemoteProxy(NetworkManagerBase networkManager, NetConnection networkConnection)
        {
            _networkManager = networkManager;
            _networkConnection = networkConnection;
        }

        public Task<object> Message(MessageViewModel message)
        {
            return _networkManager.SendCommand<object, MessageViewModel>(_networkConnection, "Message", message);
        }

        public Task<object> StartInTen()
        {
            return _networkManager.SendCommand<object>(_networkConnection, "StartInTen");
        }

        public Task<object> NewInput(InputState inputState)
        {
            return _networkManager.SendCommand<object, InputState>(_networkConnection, "NewInput", inputState);
        }
    }
}