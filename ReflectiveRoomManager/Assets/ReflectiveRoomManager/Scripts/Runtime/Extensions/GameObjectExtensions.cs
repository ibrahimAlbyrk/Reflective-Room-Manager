using UnityEngine;

namespace REFLECTIVE.Runtime.Extensions
{
    using Container.Helper;
    
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Extension method that returns a RoomContainerHelper object for a given GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject to get the RoomContainerHelper for.</param>
        /// <returns>A RoomContainerHelper object.</returns>
        public static RoomContainerHelper RoomContainer(this GameObject gameObject)
        {
            return new RoomContainerHelper(gameObject.scene);
        }
    }
}