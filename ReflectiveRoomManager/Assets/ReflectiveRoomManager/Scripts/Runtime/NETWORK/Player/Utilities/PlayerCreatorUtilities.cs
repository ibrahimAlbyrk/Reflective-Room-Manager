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
        /// <summary>
        /// Checks whether the client is a player object.
        /// If there is a player object, it replaces it with ReplacePlayer()
        /// if not, it creates it with CreatePlayer().
        /// </summary>
        /// <seealso cref="CreatePlayer"/>
        /// <seealso cref="ReplacePlayer"/>
        /// <param name="conn"></param>
        /// <param name="prefab"></param>
        /// <param name="onCompleted"></param>
        public static void TryCreatePlayerOrReplace(NetworkConnection conn, GameObject prefab, Action<GameObject> onCompleted = null)
        {
            var enumerator = conn.identity != null
                ? ReplacePlayer_Cor(conn, prefab, onCompleted)
                : CreatePlayer_Cor(conn, prefab, onCompleted);

            CoroutineRunner.Instance.StartCoroutine(enumerator);
        }

        /// <summary>
        /// Creates the specified prefab as a player object for the given connection
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="prefab"></param>
        /// <param name="onCompleted"></param>
        public static void CreatePlayer(NetworkConnection conn, GameObject prefab, Action<GameObject> onCompleted = null)
        {
            CoroutineRunner.Instance.StartCoroutine(CreatePlayer_Cor(conn, prefab, onCompleted));
        }

        /// <summary>
        /// For the given connection, it deletes the old player object and creates a new one
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="prefab"></param>
        /// <param name="onCompleted"></param>
        public static void ReplacePlayer(NetworkConnection conn, GameObject prefab, Action<GameObject> onCompleted = null)
        {
            CoroutineRunner.Instance.StartCoroutine(ReplacePlayer_Cor(conn, prefab, onCompleted));
        }

        /// <summary>
        /// If the client has a player object,
        /// it performs removal via the server and the client.
        /// </summary>
        /// <param name="conn"></param>
        public static void RemovePlayer(NetworkConnection conn)
        {
            CoroutineRunner.Instance.StartCoroutine(RemovePlayer_Cor(conn));
        }

        /// <summary>
        /// It tries to find the scene of the room via connection.
        /// Creates the specified prefab in the given scene.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="prefab"></param>
        /// <returns></returns>
        private static GameObject SpawnPlayer(NetworkConnection conn, GameObject prefab)
        {
            var room = RoomManagerBase.Instance.GetRoomByConnection(conn);

            //If there is no room, then it is in the lobby, and since it is in the lobby in this script,
            //I take the scene of its own object.
            //If you need it to go to a different scene, you can change it.
            var scene = room?.Scene ?? SceneManager.GetActiveScene();
            
            var player = NetworkSpawnUtilities.SpawnObjectForScene(scene, prefab, Vector3.zero, Quaternion.identity, conn);

            return player;
        }

        private static IEnumerator CreatePlayer_Cor(NetworkConnection conn, GameObject prefab,
            Action<GameObject> onCompleted)
        {
            if (conn is not NetworkConnectionToClient connectionToClient) yield break;
            
            var player = SpawnPlayer(conn, prefab);

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
            
            NetworkServer.RemovePlayerForConnection(connectionToClient, false);

            yield return null;
            
            if(player != null)
                NetworkServer.Destroy(player);
        }
        
    }
}