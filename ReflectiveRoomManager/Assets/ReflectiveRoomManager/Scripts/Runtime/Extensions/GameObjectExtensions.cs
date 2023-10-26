using UnityEngine;
using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.Extensions
{
    using Container.Helper;
    
    public static class GameObjectExtensions
    {
        private static Scene _scene;

        public static ContainerHelper Container(this GameObject gameObject)
        {
            return new ContainerHelper(gameObject.scene);
        }
    }
}