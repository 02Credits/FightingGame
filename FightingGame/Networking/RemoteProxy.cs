//Generated code. Manual changes will be clobbered
using FightingGame.GameLogic;
using Lidgren.Network;
using System.Threading.Tasks;

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

        public Task<object> NewInput(InputState inputState)
        {
            return _networkManager.SendCommand<object, InputState>(_networkConnection, "NewInput", inputState);
        }
    }
}