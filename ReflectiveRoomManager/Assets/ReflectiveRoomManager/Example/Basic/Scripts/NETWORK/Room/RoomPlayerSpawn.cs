﻿using Mirror;
using UnityEngine;
using System.Collections;
using REFLECTIVE.Runtime.NETWORK.Room;
using REFLECTIVE.Runtime.NETWORK.Utilities;

namespace Example.Network.Room
{
    public class RoomPlayerSpawn : MonoBehaviour
    {
        [SerializeField] private GameObject _gamePlayerPrefab;
        [SerializeField] private GameObject _lobbyPlayerPrefab;

        //If true, it creates a player object for your player in the lobby when you leave the room.
        //If your project has such a design, mark it.
        [SerializeField] private bool _useLobby;
        
        // Necessary event assignments are being made
        private void Start()
        {
            RoomManagerBase.Singleton.Events.OnServerJoinedRoom += CreateGamePlayer;
            
            if(_useLobby)
                RoomManagerBase.Singleton.Events.OnServerExitedRoom += CreateLobbyPlayer;
        }

        /// <summary>
        /// Performs player creation for the game.
        /// If there is an identity, it deletes it and creates a new one.
        /// </summary>
        /// <param name="conn"></param>
        private void CreateGamePlayer(NetworkConnection conn)
        {
            if (conn.identity != null)
            {
                StartCoroutine(ReplacePlayer(conn, _gamePlayerPrefab));

                return;
            }

            CreatePlayer(conn, _gamePlayerPrefab);
        }

        /// <summary>
        /// When the player leaves the room,
        /// he deletes his object and loads the lobby prefab.
        /// </summary>
        /// <param name="conn"></param>
        private void CreateLobbyPlayer(NetworkConnection conn)
        {
            if (conn.identity == null) return;

            StartCoroutine(ReplacePlayer(conn, _lobbyPlayerPrefab));
        }

        /// <summary>
        /// For the given connection, it deletes the old player object and creates a new one
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="prefab"></param>
        /// <returns></returns>
        private IEnumerator ReplacePlayer(NetworkConnection conn, GameObject prefab)
        {
            var oldPlayer = conn.identity.gameObject;
            
            var newPlayer = SpawnPlayer(conn, prefab);

            NetworkServer.ReplacePlayerForConnection(conn.identity.connectionToClient, newPlayer, true);

            yield return new WaitForEndOfFrame();
            
            if(oldPlayer != null)
                NetworkServer.Destroy(oldPlayer);
        }

        /// <summary>
        /// Creates the specified prefab as a player object for the given connection
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="prefab"></param>
        private void CreatePlayer(NetworkConnection conn, GameObject prefab)
        {
            if (conn is not NetworkConnectionToClient connectionToClient) return;
            
            var player = SpawnPlayer(conn, prefab);

            NetworkServer.AddPlayerForConnection(connectionToClient, player);
        }
        
        /// <summary>
        /// It tries to find the scene of the room via connection.
        /// Creates the specified prefab in the given scene.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="prefab"></param>
        /// <returns></returns>
        private GameObject SpawnPlayer(NetworkConnection conn, GameObject prefab)
        {
            var room = RoomManagerBase.Singleton.GetRoomOfPlayer(conn);

            //If there is no room, then it is in the lobby, and since it is in the lobby in this script,
            //I take the scene of its own object.
            //If you need it to go to a different scene, you can change it.
            var scene = room?.Scene ?? gameObject.scene;
            
            return NetworkSpawnUtilities.SpawnObjectForScene(scene, prefab, Vector3.zero, Quaternion.identity);
        }
    }
}