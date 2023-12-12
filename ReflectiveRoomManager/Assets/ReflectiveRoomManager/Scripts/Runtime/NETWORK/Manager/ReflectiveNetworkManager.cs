using Mirror;
using System.Linq;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Manager
{
    using Utilities;
    using Connection.Manager;
    
    [DisallowMultipleComponent]
    [AddComponentMenu("REFLECTIVE/Reflective Network Manager")]
    public class ReflectiveNetworkManager : NetworkManager
    {
        #region Start & Stop Callbacks

        public override void OnStartServer()
        {
            ReflectiveConnectionManager.networkConnections.OnServerStarted.Call();
            
            spawnPrefabs = NetworkSpawnUtilities.GetSpawnablePrefabs().ToList();
        }
        
        public override void OnStartClient()
        {
            ReflectiveConnectionManager.networkConnections.OnClientStarted.Call();
            
            //If it is a host, we do not perform this operation.
            //The reason is that transactions are already being performed on the server.
            if (NetworkServer.active)
                return;
            
            spawnPrefabs = NetworkSpawnUtilities.GetSpawnablePrefabs().ToList();

            foreach (var prefab in spawnPrefabs)
            {
                NetworkClient.RegisterPrefab(prefab);
            }
        }

        public override void OnStopServer()
        {
            ReflectiveConnectionManager.networkConnections.OnServerStopped.Call();
        }

        public override void OnStopClient()
        {
            ReflectiveConnectionManager.networkConnections.OnClientStopped.Call();;
        }

        #endregion

        #region Connection Callbacks

        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            ReflectiveConnectionManager.networkConnections.OnServerConnected.Call(conn);
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            ReflectiveConnectionManager.networkConnections.OnServerDisconnected.Call(conn);
            
            base.OnServerDisconnect(conn);
        }

        public override void OnClientConnect()
        {
            base.OnClientConnect();
            
            ReflectiveConnectionManager.networkConnections.OnClientConnected.Call();
        }

        public override void OnClientDisconnect()
        {
            ReflectiveConnectionManager.networkConnections.OnClientDisconnected.Call();
        }

        #endregion
    }
}