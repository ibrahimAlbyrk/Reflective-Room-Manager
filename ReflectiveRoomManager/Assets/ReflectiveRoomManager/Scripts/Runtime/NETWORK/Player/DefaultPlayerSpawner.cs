using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.NETWORK.Player
{
    using NETWORK.Utilities;

    public class DefaultPlayerSpawner : IPlayerSpawner
    {
        public GameObject SpawnPlayer(NetworkConnectionToClient conn, GameObject prefab, Scene roomScene)
        {
            return NetworkSpawnUtilities.SpawnObjectForScene(roomScene, prefab, Vector3.zero, Quaternion.identity, conn);
        }
    }
}
