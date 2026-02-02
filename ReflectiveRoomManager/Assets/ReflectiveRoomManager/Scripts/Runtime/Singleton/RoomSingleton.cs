using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.Singleton
{
    using Container;
    using NETWORK.Utilities;

    /// <summary>
    /// Represents a singleton instance of a room in a networked game.
    /// </summary>
    /// <typeparam name="T">The class type of the room.</typeparam>
    [RequireComponent(typeof(NetworkBehaviour))]
    public class RoomSingleton<T> : NetworkBehaviour, ISceneReady where T : class
    {
        protected virtual void Awake()
        {
            RoomContainer.Singleton.Add(gameObject.scene, this as T);
        }

        public virtual void OnSceneReady(Scene scene)
        {
            RoomContainer.Singleton.Remove<T>(gameObject.scene);
            RoomContainer.Singleton.Add(scene, this as T);
        }

        protected virtual void OnDestroy()
        {
            RoomContainer.Singleton.Remove<T>(gameObject.scene);
        }
    }
}