using System;
using Mirror;
using System.Linq;
using REFLECTIVE.Runtime.NETWORK.Utilities;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Manager
{
    [AddComponentMenu("REFLECTIVE/Reflective Network Manager")]
    public class NetworkManager : Mirror.NetworkManager
    {
        #region Events

        //Server Side
        public static event Action OnStartedServer;
        public static event Action OnStoppedServer;
        public static event Action<NetworkConnection> OnServerConnected;
        public static event Action<NetworkConnectionToClient> OnServerDisconnected;
        
        //Client Side
        public static event Action OnStartedClient;
        public static event Action OnStoppedClient;
        public static event Action OnClientConnected;
        public static event Action OnClientDisconnected;

        #endregion
        
        #region Start & Stop Callbacks

        public override void OnStartServer()
        {
            spawnPrefabs = NetworkSpawnUtilities.GetSpawnablePrefabs().ToList();
            
            OnStartedServer?.Invoke();
        }

        public override void OnStartClient()
        {
            spawnPrefabs = NetworkSpawnUtilities.GetSpawnablePrefabs().ToList();
            
            OnStartedClient?.Invoke();
        }

        public override void OnStopServer()
        {
            OnStoppedServer?.Invoke();
        }

        public override void OnStopClient()
        {
            OnStoppedClient?.Invoke();
        }

        #endregion

        #region Connection Callbacks

        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            OnServerConnected?.Invoke(conn);
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            OnServerDisconnected?.Invoke(conn);
        }

        public override void OnClientConnect()
        {
            OnClientConnected?.Invoke();
        }

        public override void OnClientDisconnect()
        {
            OnClientDisconnected?.Invoke();
        }

        #endregion
    }
}