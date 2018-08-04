//Generated code. Manual changes will be clobbered
using FightingGame.Systems;
using Lidgren.Network;
using System.Threading.Tasks;

namespace FightingGame.Networking
{
    public class RemoteProxy
    {        
        NetworkManager _networkManager;
        NetConnection _networkConnection;

        public RemoteProxy(NetworkManager networkManager, NetConnection networkConnection)
        {
            _networkManager = networkManager;
            _networkConnection = networkConnection;
        }

        public Task<object> NewInput(string playerId, int inputFrame, InputState inputState)
        {
            return _networkManager.SendCommand<object, string, int, InputState>(_networkConnection, "NewInput", playerId, inputFrame, inputState);
        }
    }
}