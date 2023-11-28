using Mirror;
using UnityEngine;

namespace REFLECTIVE.Runtime.Singleton
{
    using Container;

    /// <summary>
    /// Represents a singleton instance of a room in a networked game.
    /// </summary>
    /// <typeparam name="T">The class type of the room.</typeparam>
    [RequireComponent(typeof(NetworkBehaviour))]
    public class RoomSingleton<T> : NetworkBehaviour where T : class
    {
        protected virtual void Awake()
        {
            RoomContainer.Singleton.Add(gameObject.scene, this as T);;
        }

        protected virtual void OnDestroy()
        {
            RoomContainer.Singleton.Remove<T>(gameObject.scene);
        }
    }
}