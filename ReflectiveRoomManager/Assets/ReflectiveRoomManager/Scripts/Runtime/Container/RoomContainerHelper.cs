using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.Container.Helper
{
    public class RoomContainerHelper
    {
        private readonly Scene _scene;

        public RoomContainerHelper(Scene scene)
        {
            _scene = scene;
        }

        public bool Add<T>(T value) where T : class
        {
            return RoomContainer.Add(_scene, value);
        }

        public bool Remove<T>(T element = null) where T : class
        {
            return RoomContainer.Remove<T>(_scene);
        }

        public T Get<T>() where T : class
        {
            return RoomContainer.Get<T>(_scene);
        }
    }
}