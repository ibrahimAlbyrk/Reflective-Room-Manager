using UnityEngine;

namespace REFLECTIVE.Runtime.Extensions
{
    using Container.Helper;
    
    public static class GameObjectExtensions
    {
        private static RoomContainerHelper _roomContainerHelper;
        
        /// <summary>
        /// Extension method that returns a RoomContainerHelper object for a given GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject to get the RoomContainerHelper for.</param>
        /// <returns>A RoomContainerHelper object.</returns>
        public static RoomContainerHelper RoomContainer(this GameObject gameObject)
        {
            if (_roomContainerHelper == null)
            {
                _roomContainerHelper = new RoomContainerHelper(gameObject.scene);
            }
            else
            {
                _roomContainerHelper.Scene = gameObject.scene;
            }
            
            return _roomContainerHelper;
        }
    }
}