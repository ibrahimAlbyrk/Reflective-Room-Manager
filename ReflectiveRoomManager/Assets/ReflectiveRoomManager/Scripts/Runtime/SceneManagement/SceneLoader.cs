using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.SceneManagement
{
    using Enums;
    
    public class SceneLoader
    {
        private event Action<Scene> OnUnloading;
        
        private class SceneLoaderHandler : MonoBehaviour
        {
        }

        private static SceneLoaderHandler _sceneLoaderHandler;

        private readonly Queue<SceneLoadingTask> _scenes = new();

        private bool _isCurrentlyLoading;
        
        private class SceneLoadingTask
        {
            public readonly string SceneName;
            public readonly Action<Scene> OnTaskCompletedAction;

            public readonly LoadOperation loadOperation;
            public readonly LoadSceneMode LoadMode;
            
            public readonly Scene Scene;

            public SceneLoadingTask(string sceneName, LoadOperation loadOperation, LoadSceneMode loadMode, Action<Scene> onTaskCompleted)
            {
                SceneName = sceneName;
                OnTaskCompletedAction = onTaskCompleted;
                this.loadOperation = loadOperation;
                LoadMode = loadMode;
            }
            
            public SceneLoadingTask(Scene scene, LoadOperation loadOperation, Action<Scene> onTaskCompleted)
            {
                Scene = scene;
                OnTaskCompletedAction = onTaskCompleted;
                this.loadOperation = loadOperation;
            }
        }

        public void LoadScene(string sceneName, LoadSceneMode loadMode, Action<Scene> onCompleted = null)
        {
            Init();

            _scenes.Enqueue(new SceneLoadingTask(sceneName, LoadOperation.Load, loadMode, onCompleted));

            if (_isCurrentlyLoading) return;

            _sceneLoaderHandler.StartCoroutine(SceneCoroutine());
        }

        public void UnloadScene(Scene scene, Action<Scene> onCompleted = null, Action<Scene> onUnloading = null)
        {
            Init();

            OnUnloading += onUnloading;

            _scenes.Enqueue(new SceneLoadingTask(scene, LoadOperation.UnLoad, onCompleted));

            if (_isCurrentlyLoading) return;

            _sceneLoaderHandler.StartCoroutine(SceneCoroutine());
        }

        private IEnumerator SceneCoroutine()
        {
            while (_scenes.Count > 0)
            {
                _isCurrentlyLoading = true;
                
                var task = _scenes.Dequeue();
                
                //The reason I add one is that the first scene is the menu scene and it will stay open all the time.
                var sceneIndex = SceneManager.GetLoadedSceneCount() + (task.LoadMode == LoadSceneMode.Additive ? 1 : 0);

                Scene scene = default;
                
                //If the scene is unloading, we need to get the scene info without deleting it
                if (task.loadOperation == LoadOperation.UnLoad)
                {
                    OnUnloading?.Invoke(task.Scene);
                    OnUnloading = null;
                    scene = task.Scene;
                }

                yield return task.loadOperation switch
                {
                    LoadOperation.Load => StartAsyncSceneLoad(task),
                    LoadOperation.UnLoad => StartAsyncSceneUnload(task),
                    _ => throw new ArgumentOutOfRangeException($"SceneLoader", "Load Operation is undefined")
                };
                
                //It gives an error when loading single scenes
                //If the scene is loading, the scene information comes after loaded
                if (task.loadOperation == LoadOperation.Load)
                    scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(sceneIndex);

                task.OnTaskCompletedAction?.Invoke(scene);
            }

            _isCurrentlyLoading = false;
        }

        private static AsyncOperation StartAsyncSceneLoad(SceneLoadingTask task)
        {
            var sceneParameters = new LoadSceneParameters
            {
                loadSceneMode = task.LoadMode,
                localPhysicsMode = LocalPhysicsMode.Physics3D
            };
            
            var asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(task.SceneName, sceneParameters);

            return asyncOperation;
        }

        private static AsyncOperation StartAsyncSceneUnload(SceneLoadingTask task)
        {
            var asyncOperation =
                UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(task.Scene, UnloadSceneOptions.None);

            return asyncOperation;
        }

        private static void Init()
        {
            if (_sceneLoaderHandler != null) return;

            var gameObject = new GameObject("SceneLoader Handler");
            
            UnityEngine.Object.DontDestroyOnLoad(gameObject);
            
            _sceneLoaderHandler = gameObject.AddComponent<SceneLoaderHandler>();
        }
    }
}