using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.NETWORK.Player
{
    public interface IPlayerSpawner
    {
        GameObject SpawnPlayer(NetworkConnectionToClient conn, GameObject prefab, Scene roomScene);
    }
}
