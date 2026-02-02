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

            Manager.ReflectiveSceneManager.OnSceneLoaded += KeepLoadedScene;
            Manager.ReflectiveSceneManager.OnSceneUnloaded += DiscardLoadedScene;
        }

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

                Scene scene = default;

                switch (task.LoadOperation)
                {
                    case LoadOperation.UnLoad:
                        scene = task.Scene;
                        break;
                    case LoadOperation.Load:
                        SceneManager.sceneLoaded += OnSceneLoaded;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                yield return task.LoadOperation switch
                {
                    LoadOperation.Load => StartAsyncSceneLoad(task),
                    LoadOperation.UnLoad => StartAsyncSceneUnLoad(task),
                    _ => throw new ArgumentOutOfRangeException($"SceneLoader", "Load Operation is undefined")
                };

                if (task.LoadOperation == LoadOperation.Load)
                    SceneManager.sceneLoaded -= OnSceneLoaded;

                task.OnTaskCompletedAction?.Invoke(scene);
                continue;

                void OnSceneLoaded(Scene s, LoadSceneMode _) => scene = s;
            }

            m_loadingState.FinishLoading();

            yield return null;
        }
        
        private void KeepLoadedScene(Scene scene) => LoadedScenes.Add(scene);

        private void DiscardLoadedScene(Scene scene) => LoadedScenes.Remove(scene);
    }
}