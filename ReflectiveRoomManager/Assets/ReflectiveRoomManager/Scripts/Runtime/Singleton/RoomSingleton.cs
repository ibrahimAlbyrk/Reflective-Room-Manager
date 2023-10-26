using Mirror;

namespace REFLECTIVE.Runtime.Singleton
{
    using Container;
    
    public class RoomSingleton<T> : NetworkBehaviour where T : class
    {
        protected virtual void Awake()
        {
            Container.Add(gameObject.scene, this as T);
        }

        protected virtual void OnDestroy()
        {
            Container.Remove<T>(gameObject.scene);
        }
    }
}