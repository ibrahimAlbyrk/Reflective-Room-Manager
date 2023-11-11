using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.NETWORK.Player.Utilities
{
    using Room;
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
        public static GameObject TryCreatePlayerOrReplace(NetworkConnection conn, GameObject prefab)
        {
            return conn.identity != null
                ? ReplacePlayer(conn, prefab)
                : CreatePlayer(conn, prefab);
        }
        
        /// <summary>
        /// Creates the specified prefab as a player object for the given connection
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="prefab"></param>
        public static GameObject CreatePlayer(NetworkConnection conn, GameObject prefab)
        {
            if (conn is not NetworkConnectionToClient connectionToClient) return null;
            
            var player = SpawnPlayer(conn, prefab);

            NetworkServer.AddPlayerForConnection(connectionToClient, player);

            return player;
        }

        /// <summary>
        /// For the given connection, it deletes the old player object and creates a new one
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="prefab"></param>
        public static GameObject ReplacePlayer(NetworkConnection conn, GameObject prefab)
        {
            if (conn.identity == null) return null;
            
            var oldPlayer = conn.identity.gameObject;
            
            var newPlayer = SpawnPlayer(conn, prefab);

            NetworkServer.ReplacePlayerForConnection(conn.identity.connectionToClient, newPlayer, true);
            
            if(oldPlayer != null)
                NetworkServer.Destroy(oldPlayer);

            return newPlayer;
        }

        /// <summary>
        /// If the client has a player object,
        /// it performs removal via the server and the client.
        /// </summary>
        /// <param name="conn"></param>
        public static void RemovePlayer(NetworkConnection conn)
        {
            if (conn is not NetworkConnectionToClient connectionToClient) return;

            var player = connectionToClient.identity.gameObject;
            
            NetworkServer.RemovePlayerForConnection(conn, false);
            
            NetworkServer.Destroy(player);
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
            var room = RoomManagerBase.Singleton.GetRoomOfPlayer(conn);

            //If there is no room, then it is in the lobby, and since it is in the lobby in this script,
            //I take the scene of its own object.
            //If you need it to go to a different scene, you can change it.
            var scene = room?.Scene ?? SceneManager.GetActiveScene();
            
            var player = NetworkSpawnUtilities.SpawnObjectForScene(scene, prefab, Vector3.zero, Quaternion.identity);

            return player;
        }
    }
}