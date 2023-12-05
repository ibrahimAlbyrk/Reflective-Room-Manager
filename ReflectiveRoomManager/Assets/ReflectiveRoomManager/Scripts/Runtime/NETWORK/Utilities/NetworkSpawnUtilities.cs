using Mirror;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.NETWORK.Utilities
{
    public static class NetworkSpawnUtilities
    {
        public static GameObject SpawnObject(GameObject obj, Vector3 position, Quaternion rotation, NetworkConnection conn)
        {
            if (NetworkManager.singleton == null)
            {
                Debug.LogWarning("Network Manager is null", obj);
                return null;
            }

            var instantObj = Object.Instantiate(obj, position, rotation);
            NetworkServer.Spawn(instantObj, conn);

            return instantObj;
        }
        
        public static GameObject SpawnObject(GameObject obj, NetworkConnection conn)
        {
            if (NetworkManager.singleton == null)
            {
                Debug.LogWarning("Network Manager is null", obj);
                return null;
            }

            var instantObj = Object.Instantiate(obj, Vector3.zero, Quaternion.identity);
            NetworkServer.Spawn(instantObj, conn);

            return instantObj;
        }

        public static GameObject SpawnObjectForScene(Scene scene, GameObject obj, NetworkConnection conn = null)
        {
            var spawnedObj = SpawnObject(obj, default, default, conn);

            if (spawnedObj == null) return null;
            
            SceneManager.MoveGameObjectToScene(spawnedObj, scene);

            return spawnedObj;
        }
        
        public static GameObject SpawnObjectForScene(Scene scene, GameObject obj, Vector3 position, Quaternion rotation, NetworkConnection conn = null)
        {
            var spawnedObj = SpawnObject(obj, position, rotation, conn);

            if (spawnedObj == null) return null;
            
            SceneManager.MoveGameObjectToScene(spawnedObj, scene);

            return spawnedObj;
        }
        
        public static IEnumerable<GameObject>  GetSpawnablePrefabs()
        {
            var spawnablePrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs").ToList();

            foreach (var prefab in spawnablePrefabs.ToArray().Where(prefab => prefab == NetworkManager.singleton.playerPrefab))
            {
                spawnablePrefabs.Remove(prefab); 
            }

            return spawnablePrefabs;
        }
    }
}