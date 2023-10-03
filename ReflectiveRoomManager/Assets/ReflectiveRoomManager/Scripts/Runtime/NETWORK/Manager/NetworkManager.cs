using Mirror;
using System.Linq;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Manager
{
    using Utilities;
    using Connection.Manager;
    
    [AddComponentMenu("REFLECTIVE/Reflective Network Manager")]
    public class NetworkManager : Mirror.NetworkManager
    {
        #region Start & Stop Callbacks

        public override void OnStartServer()
        {
            spawnPrefabs = NetworkSpawnUtilities.GetSpawnablePrefabs().ToList();
            
            ConnectionManager.networkConnections.OnStartedServer?.Invoke();
        }

        public override void OnStartClient()
        {
            spawnPrefabs = NetworkSpawnUtilities.GetSpawnablePrefabs().ToList();
            
            ConnectionManager.networkConnections.OnStartedClient?.Invoke();
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
            ConnectionManager.networkConnections.OnServerDisconnected?.Invoke(conn);
        }

        public override void OnClientConnect()
        {
            ConnectionManager.networkConnections.OnClientConnected?.Invoke();
        }

        public override void OnClientDisconnect()
        {
            ConnectionManager.networkConnections.OnClientDisconnected?.Invoke();
        }

        #endregion
    }
}