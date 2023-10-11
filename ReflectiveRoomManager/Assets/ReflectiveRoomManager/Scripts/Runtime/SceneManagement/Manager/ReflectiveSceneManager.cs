﻿using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.SceneManagement.Manager
{
    using Processor;
    using NETWORK.Room;
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
            var roomData = RoomManagerBase.Singleton.RoomData;
            
            Processor = SceneProcessorFactory.Create(roomData.RoomLoaderType);
        }
    }
}