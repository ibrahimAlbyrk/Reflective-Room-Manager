using UnityEngine;

namespace REFLECTIVE.Runtime.SceneManagement.Loader
{
    using Manager;
    using MonoBehavior;

    public static class SceneLoader
    {
        public static void LoadScene()
        {
            if (ReflectiveSceneManager.Processor.GetLoadingState()) return;

            CoroutineRunner.Instance.StartCoroutine(ReflectiveSceneManager.Processor?.Process());
        }

        public static void UnloadScene()
        {
            if (ReflectiveSceneManager.Processor.GetLoadingState()) return;

            CoroutineRunner.Instance.StartCoroutine(ReflectiveSceneManager.Processor.Process());
        }
    }
}