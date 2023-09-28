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
        
        public static Action OnStartedClient;

        #endregion
        
        #region Start & Stop Callbacks

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