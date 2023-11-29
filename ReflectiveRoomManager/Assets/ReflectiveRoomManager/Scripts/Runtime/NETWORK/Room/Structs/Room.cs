using Mirror;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.NETWORK.Room.Structs
{
    using Loader;
    using Container;
    using Player.Utilities;
    using SceneManagement.Manager;

    /// <summary>
    /// Represents a room in a networked game session.
    /// </summary>
    [System.Serializable]
    public class Room
    {
        public bool IsServer;
        
        public string RoomName;

        public Scene Scene;
        
        public int MaxPlayers;
        public int CurrentPlayers;

        public readonly List<NetworkConnection> Connections;

        /// <summary>
        /// Represents a room in a network game.
        /// </summary>
        /// <param name="roomName">The name of the room.</param>
        /// <param name="maxPlayers">The maximum number of players allowed in the room.</param>
        /// <param name="isServer">A flag indicating whether this room is a server.</param>
        internal Room(string roomName, int maxPlayers, bool isServer)
        {
            IsServer = isServer;
            RoomName = roomName;
            MaxPlayers = maxPlayers;
            CurrentPlayers = 0;
            Connections = new List<NetworkConnection>();
            
            Scene = default;
        }

        #region Connection

        /// <summary>
        /// Adds a network connection to the list of connections.
        /// </summary>
        /// <param name="conn">The network connection to be added.</param>
        /// <returns>True if the connection was successfully added, false if it already exists in the list.</returns>
        internal bool AddConnection(NetworkConnection conn)
        {
            if (Connections.Contains(conn)) return false;

            Connections.Add(conn);

            CurrentPlayers++;
            
            return true;
        }

        /// <summary>
        /// Removes all connections from the current network room.
        /// </summary>
        /// <returns>A list of NetworkConnection objects that were removed.</returns>
        internal List<NetworkConnection> RemoveAllConnections()
        {
            var connections = Connections.ToList();

            var room = this;
            
            connections.ForEach(connection =>
            {
                room.RemoveConnection(connection);
            });

            return connections;
        }

        /// <summary>
        /// Removes a network connection from the list of connections and updates the count of current players.
        /// </summary>
        /// <param name="conn">The network connection to be removed. </param>
        /// <returns>True if the connection is successfully removed, false otherwise.</returns>
        internal bool RemoveConnection(NetworkConnection conn)
        {
            var isRemoved = Connections.Remove(conn);

            if (isRemoved) CurrentPlayers--;

            return isRemoved;
        }

        #endregion

        #region Scene
        
        /// <summary>
        /// Changes the current scene to the specified scene.
        /// </summary>
        /// <param name="sceneName">The name of the scene to change to.</param>
        /// <param name="keepClientObjects">Specifies whether to keep client objects when changing scenes.</param>
        internal void ChangeScene(string sceneName, bool keepClientObjects)
        {
            ReflectiveSceneManager.LoadScene(sceneName, OnSceneLoaded);
            
            return;

            void OnSceneLoaded(Scene loadedScene)
            {
                HandleSceneLoadingAndUpdateState(sceneName, keepClientObjects, ref loadedScene);
            }
        }

        /// <summary>
        /// Handles the loading of a scene and updates the state accordingly.
        /// </summary>
        /// <param name="sceneName">The name of the scene to load.</param>
        /// <param name="keepClientObjects">A boolean indicating whether client objects should be kept.</param>
        /// <param name="loadedScene">A reference to the loaded scene object.</param>
        private void HandleSceneLoadingAndUpdateState(string sceneName, bool keepClientObjects, ref Scene loadedScene)
        {
            var beforeScene = Scene;

            if (keepClientObjects)
            {
                MovePlayersToSceneAndNotify(sceneName, ref loadedScene);
            }

            Scene = loadedScene;
                
            ReflectiveSceneManager.UnLoadScene(beforeScene);
                
            RoomContainer.Listener.CallSceneListeners(loadedScene, RoomName);
        }
        
        private void MovePlayersToSceneAndNotify(string sceneName, ref Scene loadedScene)
        {
            if (RoomManagerBase.Instance.RoomLoaderType == RoomLoaderType.NoneScene) return;
            
            var clientChangeRoomSceneMessage = new ClientChangeRoomSceneMessage { SceneName = sceneName };
            
            foreach (var conn in Connections)
            {
                PlayerMoveUtilities.PlayerMoveToScene(conn, loadedScene);

                SendClientChangeSceneMessage(conn, clientChangeRoomSceneMessage);
            }   
        }
        
        private void SendClientChangeSceneMessage(NetworkConnection conn, ClientChangeRoomSceneMessage clientChangeRoomSceneMessage)
        {
            conn.Send(clientChangeRoomSceneMessage);
        }

        #endregion
    }
}