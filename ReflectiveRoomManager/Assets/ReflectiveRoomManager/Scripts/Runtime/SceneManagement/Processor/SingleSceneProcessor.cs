using System;
using System.Collections;
using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.SceneManagement.Processor
{
    using Task;
    using Enums;
    using NETWORK.Room;
    
    public class SingleSceneProcessor : SceneProcessor
    {
        private SceneLoadingTask _sceneTask;

        public SingleSceneProcessor()
        {
            m_loadSceneMode = LoadSceneMode.Single;
        }
        
        public override void LoadScene(string sceneName, Action<Scene> onCompleted = null)
        {
            _sceneTask = new SceneLoadingTask(sceneName, LoadOperation.Load, m_loadSceneMode, onCompleted);
            
            base.LoadScene(sceneName, onCompleted);
        }

        public override void UnLoadScene(Scene scene, Action<Scene> onCompleted = null)
        {
            var lobbySceneName = RoomManagerBase.Singleton.GetLobbySceneName();
            
            _sceneTask = new SceneLoadingTask(lobbySceneName, LoadOperation.Load, m_loadSceneMode, onCompleted);
            
            base.UnLoadScene(scene, onCompleted);
        }
        
        public override IEnumerator Process()
        {
            if (_sceneTask == null) yield break;
            
            m_loadingState.StartLoading();

            Scene scene = default;
            
            //If the scene is unloading, we need to get the scene info without deleting it
            if (_sceneTask.LoadOperation == LoadOperation.UnLoad)
                scene = _sceneTask.Scene;

            yield return _sceneTask.LoadOperation switch
            {
                LoadOperation.Load => StartAsyncSceneLoad(_sceneTask),
                LoadOperation.UnLoad => StartAsyncSceneUnLoad(_sceneTask),
                _ => throw new ArgumentOutOfRangeException($"SceneLoader", "Load Operation is undefined")
            };

            //If the scene is loading, the scene information comes after loaded
            if (_sceneTask.LoadOperation == LoadOperation.Load)
                scene = SceneManager.GetSceneByName(_sceneTask.SceneName);
            
            _sceneTask.OnTaskCompletedAction?.Invoke(scene);

            m_loadingState.FinishLoading();
            
            yield return null;
        }
    }
}