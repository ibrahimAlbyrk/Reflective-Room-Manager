using Mirror;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using REFLECTIVE.Runtime.NETWORK.Connection.Manager;
using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.NETWORK.Room
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

        private Dictionary<string, string> _customData;
        
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

            _customData = new Dictionary<string, string>();
        }

        #region Custom Data

        internal Dictionary<string, string> GetCustomData()
        {
            return new Dictionary<string, string>(_customData);
        }
        
        internal void SetCustomData(Dictionary<string, string> customData)
        {
            _customData = new Dictionary<string, string>(customData);
        }

        internal void SetCustomData(params (string, string)[] customData)
        {
            _customData = new Dictionary<string, string>();
            
            foreach (var (key, value) in customData)
            {
                _customData.Add(key, value);
            }
        }

        internal void AddCustomData(string key, string value)
        {
            if (_customData.TryAdd(key, value)) return;
            
            Debug.LogWarning("There is data with the same name for custom data");
        }
        
        internal bool RemoveCustomDataset(string dataName)
        {
            var isRemoved = _customData.Remove(dataName);

            return isRemoved;
        }

        internal void RemoveAllCustomData()
        {
            _customData.Clear();
        }

        #endregion

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
            PrepareSceneChange(keepClientObjects, sceneName);
            
            ReflectiveSceneManager.LoadScene(sceneName, OnSceneLoaded);
            
            return;

            void OnSceneLoaded(Scene loadedScene)
            {
                ReflectiveConnectionManager.roomConnections.OnServerRoomSceneChanged.Call(loadedScene);
                
                HandleSceneLoadingAndUpdateState(loadedScene);
                
                SpawnPlayersToScene(loadedScene);
            }
        }

        private void PrepareSceneChange(bool keepClientObjects, string sceneName)
        {
            if (keepClientObjects)
            {
                RemoveAllPlayerFromPreviousScene();
            }
        
            NotifyClientsAboutSceneChange(sceneName);
        }
        

        private void HandleSceneLoadingAndUpdateState(Scene loadedScene)
        {
            var beforeScene = Scene;
            
            Scene = loadedScene;
                
            ReflectiveSceneManager.UnLoadScene(beforeScene);
                
            RoomContainer.Listener.CallSceneChangeListeners(RoomName, loadedScene);
        }

        private void NotifyClientsAboutSceneChange(string sceneName)
        {
            var sceneMessage = CreateSceneMessage(sceneName);
                
            Connections.ForEach(conn => conn.Send(sceneMessage));
        }
        
        private SceneMessage CreateSceneMessage(string sceneName)
        {
            return new SceneMessage
            {
                sceneName = sceneName, 
                sceneOperation = SceneOperation.Normal
            };
        }
        
        private void RemoveAllPlayerFromPreviousScene()
        {
            Connections.ForEach(PlayerCreatorUtilities.RemovePlayer);
        }
        
        private void SpawnPlayersToScene(Scene loadedScene)
        {
            if (RoomManagerBase.Instance.RoomLoaderType == RoomLoaderType.NoneScene) return;
            
            foreach (var conn in Connections)
            {
                var player = PlayerCreatorUtilities.TryCreatePlayerOrReplace(conn, NetworkManager.singleton.playerPrefab);
                
                SceneManager.MoveGameObjectToScene(player, loadedScene);
            }
        }

        #endregion
    }
}