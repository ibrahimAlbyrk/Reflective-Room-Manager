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
            ConnectionManager.networkConnections.OnStartedServer?.Invoke();
            
            spawnPrefabs = NetworkSpawnUtilities.GetSpawnablePrefabs().ToList();
        }
        
        public override void OnStartClient()
        {
            ConnectionManager.networkConnections.OnStartedClient?.Invoke();
            
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
            ConnectionManager.networkConnections.OnStoppedServer?.Invoke();
        }

        public override void OnStopClient()
        {
            ConnectionManager.networkConnections.OnStoppedClient?.Invoke();
        }

        #endregion

        #region Connection Callbacks

        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            ConnectionManager.networkConnections.OnServerConnected?.Invoke(conn);
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            base.OnServerDisconnect(conn);
            
            ConnectionManager.networkConnections.OnServerDisconnected?.Invoke(conn);
        }

        public override void OnClientConnect()
        {
            base.OnClientConnect();
            
            ConnectionManager.networkConnections.OnClientConnected?.Invoke();
        }

        public override void OnClientDisconnect()
        {
            base.OnClientDisconnect();
            
            ConnectionManager.networkConnections.OnClientDisconnected?.Invoke();
        }

        #endregion
    }
}