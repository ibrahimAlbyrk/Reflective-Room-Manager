using System;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.SceneManagement.Processor
{
    using Task;
    using Loader;
    using Interface;
    using NETWORK.Room;
    
    public abstract class SceneProcessor : ISceneProcessable
    {
        protected SceneLoadingState m_loadingState { get; } = new();

        protected LoadSceneMode m_loadSceneMode;

        public bool GetLoadingState() => m_loadingState.IsCurrentlyLoading;
        
        public abstract IEnumerator Process();

        public virtual void LoadScene(string sceneName, Action<Scene> onCompleted = null)
        {
            SceneLoader.LoadScene();
        }

        public virtual void UnLoadScene(Scene scene, Action<Scene> onCompleted = null)
        {
            SceneLoader.UnloadScene();
        }
        
        protected static AsyncOperation StartAsyncSceneLoad(SceneLoadingTask task)
        {
            var sceneParameters = new LoadSceneParameters
            {
                loadSceneMode = task.LoadMode,
                localPhysicsMode = RoomManagerBase.Singleton.PhysicsMode
            };
            
            var asyncOperation = SceneManager.LoadSceneAsync(task.SceneName, sceneParameters);

            return asyncOperation;
        }

        protected static AsyncOperation StartAsyncSceneUnLoad(SceneLoadingTask task)
        {
            var asyncOperation =
                SceneManager.UnloadSceneAsync(task.Scene, UnloadSceneOptions.None);

            return asyncOperation;
        }
    }
}