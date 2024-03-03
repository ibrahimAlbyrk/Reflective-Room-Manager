using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.Container.Helper
{
    using NETWORK.Room;
    using NETWORK.Room.Listeners;
    
    public class RoomContainerHelper
    {
        private readonly Scene _scene;

        public RoomContainerHelper(Scene scene)
        {
            _scene = scene;
        }

        #region Singleton

        /// <summary>
        /// Adds a singleton instance of type T to the RoomContainer.
        /// </summary>
        /// <typeparam name="T">The type of the singleton instance.</typeparam>
        /// <param name="value">The singleton instance to add.</param>
        /// <returns>True if the singleton instance was successfully added, otherwise false.</returns>
        public bool AddSingleton<T>(T value) where T : class
        {
            return RoomContainer.Singleton.Add(_scene, value);
        }

        /// <summary>
        /// Removes a singleton instance from the room container.
        /// </summary>
        /// <typeparam name="T">The type of the singleton instance to remove.</typeparam>
        /// <param name="element">The singleton instance to remove.</param>
        /// <returns>True if the removal was successful, False otherwise.</returns>
        public bool RemoveSingleton<T>(T element = null) where T : class
        {
            return RoomContainer.Singleton.Remove<T>(_scene);
        }

        /// <summary>
        /// Retrieves a singleton instance of a specified type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the singleton instance.</typeparam>
        /// <returns>The singleton instance of type <typeparamref name="T"/>.</returns>
        public T GetSingleton<T>() where T : class
        {
            return RoomContainer.Singleton.Get<T>(_scene);
        }

        #endregion

        #region Listener

        /// <summary>
        /// Registers a listener to receive updates from a room in the scene.
        /// </summary>
        /// <remarks>The earliest method it can call is the Start method.</remarks>
        /// <param name="sceneListener">The listener object that implements the IRoomListener interface.</param>
        public void RegisterListener<T>(IRoomListener sceneListener) where T : IRoomListener
        {
            var room = RoomManagerBase.Instance.GetRoomOfScene(_scene);
            
            if (room == null) return;
            
            RoomContainer.Listener.RegisterListener<T>(room.Name, sceneListener);
        }

        /// <summary>
        /// Unregisters a listener from the specified room.
        /// </summary>
        /// <remarks>The earliest method it can call is the OnDestroy method.</remarks>
        /// <param name="sceneListener">The listener to unregister.</param>
        public void UnRegisterListener<T>(IRoomListener sceneListener) where T : IRoomListener
        {
            var room = RoomManagerBase.Instance.GetRoomOfScene(_scene);

            if (room == null) return;
            
            RoomContainer.Listener.UnRegisterListener<T>(room.Name, sceneListener);
        }

        #endregion
    }
}