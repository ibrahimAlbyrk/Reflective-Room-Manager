using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room.Scenes
{
    using Loader;
    
    internal static class RoomSceneChanger
    {
        private static ISceneChangeManager _sceneChangeManager;

        private static INetworkOperationManager _networkOperationManager;
        private static IClientManager _clientManager;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            _networkOperationManager = new NetworkOperationManager();
            _clientManager = new ClientManager(_networkOperationManager);
            
            _sceneChangeManager = new SceneChangeManager(_clientManager);
        }

        internal static void ChangeScene(Room room, string sceneName, bool keepClientObjects)
        {
            if (RoomManagerBase.Instance == null) return;

            if (RoomManagerBase.Instance.RoomLoaderType == RoomLoaderType.NoneScene) return;

            if (_sceneChangeManager == null)
            {
                Debug.LogError("SceneChangeManager not initialized");
                return;
            }

            _sceneChangeManager.ChangeScene(room, sceneName, keepClientObjects);
        }
    }
}