using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.Container.Helper
{
    public class ContainerHelper
    {
        private readonly Scene _scene;

        public ContainerHelper(Scene scene)
        {
            _scene = scene;
        }

        public bool Add<T>(T value) where T : class
        {
            return Container.Add(_scene, value);
        }

        public bool Remove<T>(T element = null) where T : class
        {
            return Container.Remove<T>(_scene);
        }

        public T Get<T>() where T : class
        {
            return Container.Get<T>(_scene);
        }
    }
}