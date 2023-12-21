using UnityEngine;

namespace Examples.SpaceShooter.Utilities
{
    public static class GameObjectUtilities
    {
        public static void Destroy(this GameObject gameObject)
        {
            Object.Destroy(gameObject);
        }
    }
}