using Mirror;
using System.Linq;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Manager
{
    [AddComponentMenu("REFLECTIVE/Reflective Network Manager")]
    public class REFLECTIVE_NetworkManager : NetworkManager
    {
        #region Start & Stop Callbacks

        public override void OnStartServer()
        {
            SetSpawnablePrefabs();
        }

        public override void OnStartClient()
        {
            SetSpawnablePrefabs();
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