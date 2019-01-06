using Lidgren.Network;
using System;

namespace FightingGame.Networking
{
    public class NetworkManager : NetworkManagerBase 
    {
        public NetworkManager(Methods methods) : base(methods) { }

        public void Connect(string host, int port)
        {
            var config = new NetPeerConfiguration("fighting-game");
            if (host.ToLower() == "localhost") host = "127.0.0.1";
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

        public override void ConnectionConnected(RemoteProxy proxy)
        {
            Console.WriteLine("Somebody connected. Do something.");
        }

        public override void ConnectionDisconnected()
        {
            Console.WriteLine("Stop game. Somebody disconnected...");
        }
    }
}