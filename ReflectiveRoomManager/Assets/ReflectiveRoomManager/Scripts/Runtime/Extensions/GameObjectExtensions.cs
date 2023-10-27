using UnityEngine;

namespace REFLECTIVE.Runtime.Extensions
{
    using Container.Helper;
    
    public static class GameObjectExtensions
    {
        public static RoomContainerHelper Container(this GameObject gameObject)
        {
            return new RoomContainerHelper(gameObject.scene);
        }
    }
}