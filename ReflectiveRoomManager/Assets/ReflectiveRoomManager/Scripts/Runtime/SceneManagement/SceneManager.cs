using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.SceneManagement
{
    public static class SceneManager
    {
        public static event Action<Scene> OnSceneLoaded;
        public static event Action<Scene> OnSceneUnloading;
        public static event Action<Scene> OnSceneUnloaded;

        private static readonly List<Scene> LoadedScenes = new();
        
        private static SceneLoader _loader;

        public static int GetLoadedSceneCount() => LoadedScenes.Count;

        #region SceneHandler Methods

        public static void LoadScene(string sceneName, LoadSceneMode loadSceneMode = LoadSceneMode.Single, Action<Scene> onCompleted = null)
        {
            _loader.LoadScene(sceneName, loadSceneMode,
                loadScene =>
                {
                    onCompleted?.Invoke(loadScene);
                    OnSceneLoaded?.Invoke(loadScene);
                });
        }

        public static void UnLoadScene(Scene scene, Action<Scene> onCompleted = null)
        {
            _loader.UnloadScene(scene, unloadScene =>
            {
                onCompleted?.Invoke(unloadScene);
                OnSceneUnloaded?.Invoke(unloadScene);
            }, OnSceneUnloading);
        }

        #endregion
        
        private static void KeepLoadedScene(Scene scene) => LoadedScenes.Add(scene);
        
        private static void DiscardLoadedScene(Scene scene) => LoadedScenes.Remove(scene);

        [RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
            OnSceneLoaded += KeepLoadedScene;
            OnSceneUnloaded += DiscardLoadedScene;

            _loader = new SceneLoader();
        }
    }
}