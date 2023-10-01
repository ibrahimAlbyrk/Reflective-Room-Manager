using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace QFSW.QC.Extras
{
    public static class SceneCommands
    {
        private static async Task PollUntilAsync(int pollInterval, Func<bool> predicate)
        {
            while (!predicate())
            {
                await Task.Delay(pollInterval);
            }
        }

        [Command("load-scene", "loads a scene by name into the game")]
        private static async Task LoadScene(string sceneName,
        [CommandParameterDescription("'Single' mode replaces the current scene with the new scene, whereas 'Additive' merges them")]LoadSceneMode loadMode = LoadSceneMode.Single)
        {
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName, loadMode);
            await PollUntilAsync(16, () => asyncOperation.isDone);
        }

        [Command("load-scene-index", "loads a scene by index into the game")]
        private static async Task LoadScene(int sceneIndex,
        [CommandParameterDescription("'Single' mode replaces the current scene with the new scene, whereas 'Additive' merges them")]LoadSceneMode loadMode = LoadSceneMode.Single)
        {
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneIndex, loadMode);
            await PollUntilAsync(16, () => asyncOperation.isDone);
        }

        [Command("unload-scene", "unloads a scene by name")]
        private static async Task UnloadScene(string sceneName)
        {
            AsyncOperation asyncOperation = SceneManager.UnloadSceneAsync(sceneName);
            await PollUntilAsync(16, () => asyncOperation.isDone);
        }

        [Command("unload-scene-index", "unloads a scene by index")]
        private static async Task UnloadScene(int sceneIndex)
        {
            AsyncOperation asyncOperation = SceneManager.UnloadSceneAsync(sceneIndex);
            await PollUntilAsync(16, () => asyncOperation.isDone);
        }

        private static IEnumerable<Scene> GetScenesInBuild()
        {
            int sceneCount = SceneManager.sceneCountInBuildSettings;
            for (int i = 0; i < sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneByBuildIndex(i);
                yield return scene;
            }
        }

        [Command("all-scenes", "gets the name and index of every scene included in the build")]
        private static Dictionary<int, string> GetAllScenes()
        {
            Dictionary<int, string> sceneData = new Dictionary<int, string>();
            int sceneCount = SceneManager.sceneCountInBuildSettings;
            for (int i = 0; i < sceneCount; i++)
            {
                int sceneIndex = i;
                string scenePath = SceneUtility.GetScenePathByBuildIndex(sceneIndex);
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

                sceneData.Add(sceneIndex, sceneName);
            }

            return sceneData;
        }

        [Command("loaded-scenes", "gets the name and index of every scene currently loaded")]
        private static Dictionary<int, string> GetLoadedScenes()
        {
            IEnumerable<Scene> loadedScenes = GetScenesInBuild().Where(x => x.isLoaded);
            Dictionary<int, string> sceneData = loadedScenes.ToDictionary(x => x.buildIndex, x => x.name);
            return sceneData;
        }

        [Command("active-scene", "gets the name of the active primary scene")]
        private static string GetCurrentScene()
        {
            Scene scene = SceneManager.GetActiveScene();
            return scene.name;
        }

        [Command("set-active-scene", "sets the active scene to the scene with name 'sceneName'")]
        private static void SetActiveScene(string sceneName)
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.isLoaded) { throw new ArgumentException($"Scene {sceneName} must be loaded before it can be set active"); }

            SceneManager.SetActiveScene(scene);
        }
    }
}
