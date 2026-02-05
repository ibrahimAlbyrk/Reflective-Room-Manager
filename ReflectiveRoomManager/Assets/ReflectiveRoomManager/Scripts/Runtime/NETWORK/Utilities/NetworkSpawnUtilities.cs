#if REFLECTIVE_SERVER
using Mirror;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.NETWORK.Utilities
{
    public static class NetworkSpawnUtilities
    {
        public static GameObject SpawnObject(GameObject obj, NetworkConnectionToClient conn = null)
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
        
        public static GameObject SpawnObject(GameObject obj, Transform parent, NetworkConnectionToClient conn = null)
        {
            if (NetworkManager.singleton == null)
            {
                Debug.LogWarning("Network Manager is null", obj);
                return null;
            }

            var instantObj = Object.Instantiate(obj, Vector3.zero, Quaternion.identity, parent);
            NetworkServer.Spawn(instantObj, conn);

            return instantObj;
        }

        public static GameObject SpawnObject(GameObject obj, Vector3 position, Quaternion rotation, NetworkConnectionToClient conn = null)
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
        
        public static GameObject SpawnObject(GameObject obj, Vector3 position, Quaternion rotation, Transform parent, NetworkConnectionToClient conn = null)
        {
            if (NetworkManager.singleton == null)
            {
                Debug.LogWarning("Network Manager is null", obj);
                return null;
            }

            var instantObj = Object.Instantiate(obj, position, rotation, parent);
            NetworkServer.Spawn(instantObj, conn);

            return instantObj;
        }

        public static GameObject SpawnObjectForScene(Scene scene, GameObject obj, NetworkConnectionToClient conn = null)
        {
            var spawnedObj = SpawnObject(obj, conn);

            if (spawnedObj == null) return null;

            SceneManager.MoveGameObjectToScene(spawnedObj, scene);

            NotifySceneReady(spawnedObj, scene);

            return spawnedObj;
        }

        public static GameObject SpawnObjectForScene(Scene scene, GameObject obj, Vector3 position, Quaternion rotation, NetworkConnectionToClient conn = null)
        {
            var spawnedObj = SpawnObject(obj, position, rotation, conn);

            if (spawnedObj == null) return null;

            SceneManager.MoveGameObjectToScene(spawnedObj, scene);

            NotifySceneReady(spawnedObj, scene);

            return spawnedObj;
        }

        private static void NotifySceneReady(GameObject obj, Scene scene)
        {
            var identity = obj.GetComponent<NetworkIdentity>();

            if (identity == null) return;

            foreach (var nb in identity.NetworkBehaviours)
                if (nb is ISceneReady listener)
                    listener.OnSceneReady(scene);
        }
        
        public static IEnumerable<GameObject> GetSpawnablePrefabs()
        {
            return Resources.LoadAll<GameObject>("SpawnablePrefabs")
                .Where(p => p != NetworkManager.singleton.playerPrefab);
        }
    }
}
#endif