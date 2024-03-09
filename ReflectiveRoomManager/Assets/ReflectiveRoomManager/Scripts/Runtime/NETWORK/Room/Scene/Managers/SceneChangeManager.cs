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

        private void AddSceneChangeHandler(Room room, string sceneName)
        {
            var sceneChangeHandler = new SceneChangeHandler
            {
                Room = room,
                SceneName = sceneName
            };
            
            _sceneChangeHandlers.Add(room.ID, sceneChangeHandler);
        }
        
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
        
        private void LoadScene(uint roomID)
        {
            var sceneChangeHandler = _sceneChangeHandlers[roomID];
            
            var room = sceneChangeHandler.Room;
            var sceneName = sceneChangeHandler.SceneName;
            
            NotifyClientsAboutSceneChange(room, sceneName);
            ReflectiveSceneManager.LoadScene(sceneName, scene => OnSceneChanged(room, scene));
        }
        
        private void OnSceneChanged(Room room, Scene loadedScene)
        {
            ReflectiveConnectionManager.roomConnections.OnServerRoomSceneChanged.Call(loadedScene);
            
            if(_keepClientObjects)
                _clientManager.MoveClientsToScene(_garbageObjects, loadedScene);
            
            room.Connections.ForEach(conn => conn.Send(new SceneLoadMessage{Identities = _garbageObjects}));
            
            HandleSceneLoadingAndUpdateState(room, loadedScene);

            _sceneChangeHandlers.Remove(room.ID);
        }
        
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