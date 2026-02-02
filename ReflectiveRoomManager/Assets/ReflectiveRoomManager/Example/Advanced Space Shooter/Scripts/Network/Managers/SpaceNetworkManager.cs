using System;
using Mirror;
using REFLECTIVE.Runtime.NETWORK.Room;
using REFLECTIVE.Runtime.NETWORK.Room.Structs;
using REFLECTIVE.Runtime.NETWORK.Manager;
using REFLECTIVE.Runtime.NETWORK.Room.Service;

namespace Examples.SpaceShooter.Network.Managers
{
    public class SpaceNetworkManager : ReflectiveNetworkManager
    {
        public event Action<NetworkConnection> OnServerReadied;

        public override void OnStartServer()
        {
            base.OnStartServer();
            
            CreateOpenWorldRoom();
        }

        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            if (numPlayers >= maxConnections)
            {
                conn.Disconnect();
                return;
            }

            base.OnServerConnect(conn);
        }

        public override void OnServerReady(NetworkConnectionToClient conn)
        {
            base.OnServerReady(conn);
            
            OnServerReadied?.Invoke(conn);
        }
        
        private static void CreateOpenWorldRoom()
        {
            var sceneName = RoomManagerBase.Instance.RoomScene;

            var roomInfo = new RoomBuilder("OpenWorld", sceneName)
                .WithMaxPlayers(100)
                .Build();

            RoomServer.CreateRoom(roomInfo);
        }
    }
}