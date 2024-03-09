using Mirror;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.NETWORK.Room.Scenes
{
    using Enums;
    using Utilities;
    using Container;
    using Connection.Manager;
    using SceneManagement.Manager;
    
    internal class SceneChangeManager : ISceneChangeManager
    {
        private readonly IClientManager _clientManager;
        
        private readonly Dictionary<uint, SceneChangeHandler> _sceneChangeHandlers;
        private readonly List<NetworkIdentity> _garbageObjects;

        private bool _keepClientObjects;

        public SceneChangeManager(IClientManager clientManager)
        {
            _clientManager = clientManager;
            
            _garbageObjects = new List<NetworkIdentity>();
            _sceneChangeHandlers = new Dictionary<uint, SceneChangeHandler>();
            
            ReflectiveConnectionManager.roomConnections.OnClientSceneLoaded.AddListener(msg =>
            {
                _clientManager.ResetClientsTransformForClient(msg.Identities);
            });
        }
        
        
        public void ChangeScene(Room room, string sceneName, bool keepClientObjects)
        {
            if (_sceneChangeHandlers.ContainsKey(room.ID)) return;

            AddSceneChangeHandler(room, sceneName);
            
            _garbageObjects.Clear();
            
            PrepareSceneChange(room.ID, keepClientObjects);
        }

        /// <summary>
        /// Adds a scene change handler for the given room and scene name.
        /// </summary>
        /// <param name="room">The room to add the scene change handler to.</param>
        /// <param name="sceneName">The name of the scene.</param>
        private void AddSceneChangeHandler(Room room, string sceneName)
        {
            var sceneChangeHandler = new SceneChangeHandler
            {
                Room = room,
                SceneName = sceneName
            };
            
            _sceneChangeHandlers.Add(room.ID, sceneChangeHandler);
        }

        /// <summary>
        /// Prepares for a scene change by making necessary adjustments to hide client objects.
        /// </summary>
        /// <param name="roomID">The ID of the room.</param>
        /// <param name="keepClientObjects">A boolean indicating whether to keep client objects during the scene change.</param>
        private void PrepareSceneChange(uint roomID, bool keepClientObjects)
        {
            _keepClientObjects = keepClientObjects;

            var sceneChangeHandler = _sceneChangeHandlers[roomID];
            
            if(_keepClientObjects)
                _clientManager.KeepAllClients(_garbageObjects, sceneChangeHandler);
            else
                _clientManager.RemoveAllClients(sceneChangeHandler);
            
            LoadScene(roomID);
        }

        /// <summary>
        /// Notifies all clients in the room about a scene change.
        /// </summary>
        /// <param name="room">The room where the scene change occurred.</param>
        /// <param name="sceneName">The name of the new scene.</param>
        private void NotifyClientsAboutSceneChange(Room room, string sceneName)
        {
            var unloadBeforeSceneMessage = new SceneMessage
            {
                sceneName = room.Scene.name,
                sceneOperation = SceneOperation.UnloadAdditive,
            };
            
            var loadSceneMessage = new SceneMessage
            {
                sceneName = sceneName, 
                sceneOperation = SceneOperation.LoadAdditive,
            };
                
            room.Connections.ForEach(conn => conn.Send(unloadBeforeSceneMessage));
            room.Connections.ForEach(conn => conn.Send(loadSceneMessage));
        }

        /// <summary>
        /// Loads the specified scene for the given room.
        /// </summary>
        /// <param name="roomID">The ID of the room.</param>
        private void LoadScene(uint roomID)
        {
            var sceneChangeHandler = _sceneChangeHandlers[roomID];
            
            var room = sceneChangeHandler.Room;
            var sceneName = sceneChangeHandler.SceneName;
            
            NotifyClientsAboutSceneChange(room, sceneName);
            ReflectiveSceneManager.LoadScene(sceneName, scene => OnSceneChanged(room, scene));
        }

        /// <summary>
        /// Handler called when a scene has changed for a specific room.
        /// </summary>
        /// <param name="room">The room for which the scene has changed.</param>
        /// <param name="loadedScene">The new loaded scene.</param>
        private void OnSceneChanged(Room room, Scene loadedScene)
        {
            ReflectiveConnectionManager.roomConnections.OnServerRoomSceneChanged.Call(loadedScene);
            
            if(_keepClientObjects)
                _clientManager.MoveClientsToScene(_garbageObjects, loadedScene);
            
            room.Connections.ForEach(conn => conn.Send(new SceneLoadMessage{Identities = _garbageObjects}));
            
            HandleSceneLoadingAndUpdateState(room, loadedScene);

            _sceneChangeHandlers.Remove(room.ID);
        }

        /// <summary>
        /// Handles the loading of a scene and updates the state of the room.
        /// </summary>
        /// <param name="room">The room to handle scene loading and state update for.</param>
        /// <param name="loadedScene">The loaded scene.</param>
        private void HandleSceneLoadingAndUpdateState(Room room, Scene loadedScene)
        {
            var beforeScene = room.Scene;
            
            room.Scene = loadedScene;
            
            RoomMessageUtility.SenRoomUpdateMessage(RoomListUtility.ConvertToRoomList(room), RoomMessageState.Update);
                
            ReflectiveSceneManager.UnLoadScene(beforeScene);
                
            RoomContainer.Listener.CallSceneChangeListeners(room.Name, loadedScene);
        }
    }
}