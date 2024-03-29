﻿using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.SceneManagement.Manager
{
    using Processor;
    using NETWORK.Room;
    using NETWORK.Room.Loader;
    using Processor.Factory;
    
    public static class ReflectiveSceneManager
    {
        public static event Action<Scene> OnSceneLoaded;
        public static event Action<Scene> OnSceneUnloaded;
        
        public static SceneProcessor Processor;

        public static void LoadScene(string sceneName, Action<Scene> onCompleted = null)
        {
            Processor.LoadScene(sceneName, loadScene =>
            {
                onCompleted?.Invoke(loadScene);
                OnSceneLoaded?.Invoke(loadScene);
            });
        }

        public static void UnLoadScene(Scene scene, Action<Scene> onCompleted = null)
        {
            Processor.UnLoadScene(scene, loadScene =>
            {
                onCompleted?.Invoke(loadScene);
                OnSceneUnloaded?.Invoke(loadScene);
            });
        }

        [RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
            var roomLoaderType = RoomLoaderType.AdditiveScene;

            if (RoomManagerBase.Instance != null)
                roomLoaderType = RoomManagerBase.Instance.RoomLoaderType;
            
            Processor = SceneProcessorFactory.Create(roomLoaderType);
        }
    }
}