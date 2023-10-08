using UnityEngine;

namespace REFLECTIVE.Runtime.SceneManagement.Loader
{
    using Manager;
    using MonoBehavior;

    public static class SceneLoader
    {
        private static MonoBehaviourHook _monoBehaviourHook;

        public static void LoadScene()
        {
            Init();

            if (SceneManager.Processor.GetLoadingState()) return;

            _monoBehaviourHook.StartCoroutine(SceneManager.Processor?.Process());
        }

        public static void UnloadScene()
        {
            Init();

            if (SceneManager.Processor.GetLoadingState()) return;

            _monoBehaviourHook.StartCoroutine(SceneManager.Processor.Process());
        }

        private static void Init()
        {
            if (_monoBehaviourHook != null) return;

            var gameObject = new GameObject("SceneLoader Handler");
            
            Object.DontDestroyOnLoad(gameObject);
            
            _monoBehaviourHook = gameObject.AddComponent<MonoBehaviourHook>();
        }
    }
}