using System;
using Mirror;
using System.Linq;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Manager
{
    [AddComponentMenu("REFLECTIVE/Reflective Network Manager")]
    public class REFLECTIVE_NetworkManager : NetworkManager
    {
        #region Events

        public static Action OnStartedServer;
        public static Action OnStoppedServer;
        public static Action<NetworkConnection> OnServerConnected;
        
        public static Action OnStartedClient;
        public static Action OnClientDisconnected;

        #endregion
        
        #region Start & Stop Callbacks

        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            OnServerConnected?.Invoke(conn);
        }

        public override void OnStartServer()
        {
            SetSpawnablePrefabs();
            
            OnStartedServer?.Invoke();
        }

        public override void OnStartClient()
        {
            SetSpawnablePrefabs();
            
            OnStartedClient?.Invoke();
        }

        public override void OnStopServer()
        {
            OnStoppedServer?.Invoke();
        }

        public override void OnClientDisconnect()
        {
            OnClientDisconnected?.Invoke();
        }

        #endregion

        #region Utilities

        private void SetSpawnablePrefabs()
        {
            spawnPrefabs.Clear();
            var spawnablePrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs");

            spawnPrefabs = spawnablePrefabs.ToList();
        }

        #endregion
    }
}