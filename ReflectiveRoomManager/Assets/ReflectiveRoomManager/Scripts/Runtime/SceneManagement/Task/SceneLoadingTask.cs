using System;
using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.SceneManagement.Task
{
    using Enums;
    
    public class SceneLoadingTask
    {
        public readonly string SceneName;
        public readonly Action<Scene> OnTaskCompletedAction;

        public readonly LoadOperation LoadOperation;
        public readonly LoadSceneMode LoadMode;
            
        public readonly Scene Scene;

        public SceneLoadingTask(string sceneName, LoadOperation loadOperation, LoadSceneMode loadMode, Action<Scene> onTaskCompleted)
        {
            SceneName = sceneName;
            OnTaskCompletedAction = onTaskCompleted;
            LoadOperation = loadOperation;
            LoadMode = loadMode;
        }
            
        public SceneLoadingTask(Scene scene, LoadOperation loadOperation, Action<Scene> onTaskCompleted)
        {
            Scene = scene;
            OnTaskCompletedAction = onTaskCompleted;
            LoadOperation = loadOperation;
            LoadMode = LoadSceneMode.Single;
        }
    }
}