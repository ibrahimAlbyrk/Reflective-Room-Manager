using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.SceneManagement
{
    using Enums;
    
    public class REFLECTIVE_SceneLoader
    {
        private event Action<Scene> OnUnloading;
        
        private class SceneLoaderHandler : MonoBehaviour
        {
        }

        private static SceneLoaderHandler _sceneLoaderHandler;

        private readonly Queue<SceneLoadingTask> _scenes = new();

        private bool _isCurrentlyLoading;

        public struct SceneTask
        {
            public Scene Scene;
            public REFLECTIVE_LoadOperation reflectiveLoadOperation;

            public readonly GameObject[] RootGameObjects;

            public SceneTask(Scene scene = default, REFLECTIVE_LoadOperation reflectiveLoadOperation = REFLECTIVE_LoadOperation.Load, GameObject[] rootGameObjects = null)
            {
                Scene = scene;
                this.reflectiveLoadOperation = reflectiveLoadOperation;
                
                RootGameObjects = rootGameObjects;
            }
        }
        
        private class SceneLoadingTask
        {
            public readonly string SceneName;
            public readonly Action<Scene> OnTaskCompletedAction;

            public readonly REFLECTIVE_LoadOperation reflectiveLoadOperation;
            public readonly LoadSceneMode LoadMode;
            
            public readonly Scene Scene;

            public SceneLoadingTask(string sceneName, REFLECTIVE_LoadOperation reflectiveLoadOperation, LoadSceneMode loadMode, Action<Scene> onTaskCompleted)
            {
                SceneName = sceneName;
                OnTaskCompletedAction = onTaskCompleted;
                this.reflectiveLoadOperation = reflectiveLoadOperation;
                LoadMode = loadMode;
            }
            
            public SceneLoadingTask(Scene scene, REFLECTIVE_LoadOperation reflectiveLoadOperation, Action<Scene> onTaskCompleted)
            {
                Scene = scene;
                OnTaskCompletedAction = onTaskCompleted;
                this.reflectiveLoadOperation = reflectiveLoadOperation;
            }
        }

        public void LoadScene(string sceneName, LoadSceneMode loadMode, Action<Scene> onCompleted = null)
        {
            Init();

            _scenes.Enqueue(new SceneLoadingTask(sceneName, REFLECTIVE_LoadOperation.Load, loadMode, onCompleted));

            if (_isCurrentlyLoading) return;

            _sceneLoaderHandler.StartCoroutine(SceneCoroutine());
        }

        public void UnloadScene(Scene scene, Action<Scene> onCompleted = null, Action<Scene> onUnloading = null)
        {
            Init();

            OnUnloading += onUnloading;

            _scenes.Enqueue(new SceneLoadingTask(scene, REFLECTIVE_LoadOperation.UnLoad, onCompleted));

            if (_isCurrentlyLoading) return;

            _sceneLoaderHandler.StartCoroutine(SceneCoroutine());
        }

        private IEnumerator SceneCoroutine()
        {
            while (_scenes.Count > 0)
            {
                _isCurrentlyLoading = true;

                //The reason I add one is that the first scene is the menu scene and it will stay open all the time.
                var sceneIndex = REFLECTIVE_SceneManager.GetLoadedSceneCount() + 1;
                
                var task = _scenes.Dequeue();

                Scene scene = default;
                
                //If the scene is unloading, we need to get the scene info without deleting it
                if (task.reflectiveLoadOperation == REFLECTIVE_LoadOperation.UnLoad)
                {
                    OnUnloading?.Invoke(task.Scene);
                    OnUnloading = null;
                    scene = task.Scene;
                }

                yield return task.reflectiveLoadOperation switch
                {
                    REFLECTIVE_LoadOperation.Load => StartAsyncSceneLoad(task),
                    REFLECTIVE_LoadOperation.UnLoad => StartAsyncSceneUnload(task),
                    _ => throw new ArgumentOutOfRangeException($"SceneLoader", "Load Operation is undefined")
                };
                
                //If the scene is loading, the scene information comes after loaded
                if (task.reflectiveLoadOperation == REFLECTIVE_LoadOperation.Load)
                    scene = SceneManager.GetSceneAt(sceneIndex);

                task.OnTaskCompletedAction?.Invoke(scene);
            }

            _isCurrentlyLoading = false;
        }

        private AsyncOperation StartAsyncSceneLoad(SceneLoadingTask task)
        {
            var sceneParameters = new LoadSceneParameters
            {
                loadSceneMode = task.LoadMode,
                localPhysicsMode = LocalPhysicsMode.Physics3D
            };
            
            var asyncOperation = SceneManager.LoadSceneAsync(task.SceneName, sceneParameters);

            return asyncOperation;
        }

        private AsyncOperation StartAsyncSceneUnload(SceneLoadingTask task)
        {
            var asyncOperation =
                SceneManager.UnloadSceneAsync(task.Scene, UnloadSceneOptions.None);

            return asyncOperation;
        }

        private static void Init()
        {
            if (_sceneLoaderHandler != null) return;

            var gameObject = new GameObject("SceneLoader Handler");
            _sceneLoaderHandler = gameObject.AddComponent<SceneLoaderHandler>();
        }
    }
}