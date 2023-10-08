using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.SceneManagement.Processor
{
    using Task;
    using Enums;
    
    public class AdditiveSceneProcessor : SceneProcessor
    {
        private readonly Queue<SceneLoadingTask> _scenes = new();
        
        private readonly List<Scene> LoadedScenes = new();

        public AdditiveSceneProcessor()
        {
            m_loadSceneMode = LoadSceneMode.Additive;
        }
        
        public void KeepLoadedScene(Scene scene)
        {
            LoadedScenes.Add(scene);
        }

        public void DiscardLoadedScene(Scene scene) => LoadedScenes.Remove(scene);

        public override void LoadScene(string sceneName, Action<Scene> onCompleted = null)
        {
            _scenes.Enqueue(new SceneLoadingTask(sceneName, LoadOperation.Load, m_loadSceneMode, onCompleted));
            
            base.LoadScene(sceneName, onCompleted);
        }

        public override void UnLoadScene(Scene scene, Action<Scene> onCompleted = null)
        {
            _scenes.Enqueue(new SceneLoadingTask(scene, LoadOperation.UnLoad, onCompleted));
            
            base.UnLoadScene(scene, onCompleted);
        }

        public override IEnumerator Process()
        {
            while (_scenes.Count > 0)
            {
                m_loadingState.StartLoading();
                
                var task = _scenes.Dequeue();
                
                //The reason I add one is that the first scene is the menu scene and it will stay open all the time.
                var sceneIndex = LoadedScenes.Count + (task.LoadMode == LoadSceneMode.Additive ? 1 : 0);
                
                Scene scene = default;
                
                //If the scene is unloading, we need to get the scene info without deleting it
                if (task.LoadOperation == LoadOperation.UnLoad)
                    scene = task.Scene;
                
                yield return task.LoadOperation switch
                {
                    LoadOperation.Load => StartAsyncSceneLoad(task),
                    LoadOperation.UnLoad => StartAsyncSceneUnLoad(task),
                    _ => throw new ArgumentOutOfRangeException($"SceneLoader", "Load Operation is undefined")
                };
                
                //If the scene is loading, the scene information comes after loaded
                if (task.LoadOperation == LoadOperation.Load)
                    scene = SceneManager.GetSceneAt(sceneIndex);

                task.OnTaskCompletedAction?.Invoke(scene);
            }

            m_loadingState.FinishLoading();

            yield return null;
        }
    }
}