using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.NETWORK.Room.Scenes
{
    using SceneManagement.Manager;
    
    public class SceneSynchronizer
    {
        public SceneSynchronizer()
        {
            ReflectiveSceneManager.OnSceneLoaded += DoSyncScene;
        }

        public void DoSyncScene(Scene scene)
        {
            if (!RoomManagerBase.Singleton.AutomaticallySyncScene)
                return;
        }
    }
}