using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.NETWORK.Room.Listeners
{
    /// <summary>
    /// Represents a listener for room scene change events.
    /// </summary>
    public interface IRoomSceneListener : IRoomListener
    {
        public void OnRoomSceneListener(Scene scene);
    }
}