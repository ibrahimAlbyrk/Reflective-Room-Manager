using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.NETWORK.Room.Listeners
{
    /// <summary>
    /// Represents a listener for room scene change events.
    /// </summary>
    public interface IRoomListener
    {
        public void OnRoomSceneChanged(Scene scene);
    }
}