using Mirror;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.NETWORK.Room.Scenes
{
    using Loader;
    using Container;
    using Player.Utilities;
    using Connection.Manager;
    using SceneManagement.Manager;
    
    internal static class RoomSceneChanger
    {
        private static List<GameObject> _garbageObjects;
        
        /// <summary>
        /// Changes the current scene to the specified scene.
        /// </summary>
        /// <param name="room"></param>
        /// <param name="sceneName">The name of the scene to change to.</param>
        /// <param name="keepClientObjects">Specifies whether to keep client objects when changing scenes.</param>
        internal static void ChangeScene(Room room, string sceneName, bool keepClientObjects)
        {
            PrepareSceneChange(room, keepClientObjects, sceneName);
            
            ReflectiveSceneManager.LoadScene(sceneName, OnSceneLoaded);
            
            return;

            void OnSceneLoaded(Scene loadedScene)
            {
                ReflectiveConnectionManager.roomConnections.OnServerRoomSceneChanged.Call(loadedScene);
                
                HandleSceneLoadingAndUpdateState(room, loadedScene);
                
                SpawnPlayersToScene(room, loadedScene);
            }
        }

        private static void PrepareSceneChange(Room room, bool keepClientObjects, string sceneName)
        {
            if (keepClientObjects)
            {
                RemoveAllPlayerFromPreviousScene(room);
            }
        
            NotifyClientsAboutSceneChange(room, sceneName);
        }
        

        private static void HandleSceneLoadingAndUpdateState(Room room, Scene loadedScene)
        {
            var beforeScene = room.Scene;
            
            room.Scene = loadedScene;
                
            ReflectiveSceneManager.UnLoadScene(beforeScene);
                
            RoomContainer.Listener.CallSceneChangeListeners(room.Name, loadedScene);
        }

        private static void NotifyClientsAboutSceneChange(Room room, string sceneName)
        {
            var sceneMessage = CreateSceneMessage(sceneName);
                
            room.Connections.ForEach(conn => conn.Send(sceneMessage));
        }
        
        private static SceneMessage CreateSceneMessage(string sceneName)
        {
            return new SceneMessage
            {
                sceneName = sceneName, 
                sceneOperation = SceneOperation.Normal
            };
        }
        
        private static void RemoveAllPlayerFromPreviousScene(Room room)
        {
            room.Connections.ForEach(PlayerCreatorUtilities.RemovePlayer);
        }
        
        private static void SpawnPlayersToScene(Room room, Scene loadedScene)
        {
            if (RoomManagerBase.Instance.RoomLoaderType == RoomLoaderType.NoneScene) return;
            
            foreach (var conn in room.Connections)
            {
                PlayerCreatorUtilities.TryCreatePlayerOrReplace(conn, NetworkManager.singleton.playerPrefab,
                    spawnedPlayer =>
                    {
                        SceneManager.MoveGameObjectToScene(spawnedPlayer, loadedScene);   
                    });
            }
        }
    }
}