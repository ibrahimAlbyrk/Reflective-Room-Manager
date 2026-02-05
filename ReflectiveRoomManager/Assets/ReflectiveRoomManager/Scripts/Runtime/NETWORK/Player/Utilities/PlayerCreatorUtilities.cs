#if REFLECTIVE_SERVER
using System;
using Mirror;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.NETWORK.Player.Utilities
{
    using Room;
    using MonoBehavior;
    using NETWORK.Utilities;

    public static class PlayerCreatorUtilities
    {
        private static IPlayerSpawner _spawner = new DefaultPlayerSpawner();

        public static void SetSpawner(IPlayerSpawner spawner)
        {
            _spawner = spawner ?? new DefaultPlayerSpawner();
        }

        public static void TryCreatePlayerOrReplace(NetworkConnection conn, GameObject prefab, Action<GameObject> onCompleted = null)
        {
            var enumerator = conn.identity != null
                ? ReplacePlayer_Cor(conn, prefab, onCompleted)
                : CreatePlayer_Cor(conn, prefab, onCompleted);

            CoroutineRunner.Instance.StartCoroutine(enumerator);
        }

        public static void CreatePlayer(NetworkConnection conn, GameObject prefab, Action<GameObject> onCompleted = null)
        {
            CoroutineRunner.Instance.StartCoroutine(CreatePlayer_Cor(conn, prefab, onCompleted));
        }

        public static void ReplacePlayer(NetworkConnection conn, GameObject prefab, Action<GameObject> onCompleted = null)
        {
            CoroutineRunner.Instance.StartCoroutine(ReplacePlayer_Cor(conn, prefab, onCompleted));
        }

        public static void RemovePlayer(NetworkConnection conn)
        {
            CoroutineRunner.Instance.StartCoroutine(RemovePlayer_Cor(conn));
        }

        private static GameObject SpawnPlayer(NetworkConnectionToClient conn, GameObject prefab)
        {
            var room = RoomManagerBase.Instance.GetRoomByConnection(conn);
            var scene = room?.Scene ?? SceneManager.GetActiveScene();

            return _spawner.SpawnPlayer(conn, prefab, scene);
        }

        private static IEnumerator CreatePlayer_Cor(NetworkConnection conn, GameObject prefab,
            Action<GameObject> onCompleted)
        {
            if (conn is not NetworkConnectionToClient connectionToClient) yield break;

            var player = SpawnPlayer(connectionToClient, prefab);

            NetworkServer.AddPlayerForConnection(connectionToClient, player);

            yield return null;

            onCompleted?.Invoke(player);
        }

        private static IEnumerator ReplacePlayer_Cor(NetworkConnection conn, GameObject prefab, Action<GameObject> onCompleted)
        {
            yield return RemovePlayer_Cor(conn);

            yield return CreatePlayer_Cor(conn, prefab, onCompleted);
        }

        private static IEnumerator RemovePlayer_Cor(NetworkConnection conn)
        {
            if (conn is not NetworkConnectionToClient connectionToClient) yield break;

            var player = connectionToClient.identity.gameObject;

            NetworkServer.RemovePlayerForConnection(connectionToClient, RemovePlayerOptions.KeepActive);

            yield return null;

            if(player != null)
                NetworkServer.Destroy(player);
        }
    }
}
#endif
