using Mirror;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.NETWORK.Utilities
{
    public static class REFLECTIVE_NetworkSpawnUtilities
    {
        public static GameObject SpawnObject(GameObject obj, Vector3 position = default, Quaternion rotation = default)
        {
            if (NetworkManager.singleton == null)
            {
                Debug.LogWarning("Network Manager is null", obj);
                return null;
            }

            var instantObj = Object.Instantiate(obj, position, rotation);
            NetworkServer.Spawn(instantObj);

            return instantObj;
        }

        public static GameObject SpawnObjectForScene(Scene scene, GameObject obj, Vector3 position = default, Quaternion rotation = default)
        {
            var spawnedObj = SpawnObject(obj, position, rotation);
            SceneManager.MoveGameObjectToScene(spawnedObj, scene);

            return spawnedObj;
        }
        
        public static IEnumerable<GameObject>  GetSpawnablePrefabs()
        {
            var spawnablePrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs");

            return spawnablePrefabs;
        }
    }
}