using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.Container
{
    using Data;
    using NETWORK.Room.Listeners;
    
    internal static class RoomContainer
    {
        public static readonly SingletonContainer Singleton = new();

        public static readonly ListenerContainer Listener = new();
    }

    internal class SingletonContainer
    {
        private readonly Dictionary<Scene, RoomContainerData> _data = new();

        /// <summary>
        /// Adds an element of type T to the specified room.
        /// </summary>
        /// <param name="scene">The scene to add the element to.</param>
        /// <param name="element">The element to be added.</param>
        /// <typeparam name="T">The type of the element to be added.</typeparam>
        /// <returns>True if the element was successfully added; false if the scene already contains an element of the same type.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either the scene or the element is null.</exception>
        internal bool Add<T>(Scene scene, T element) where T : class
        {
            if (scene == default) throw new ArgumentNullException(nameof(scene));
            if (element == null) throw new ArgumentNullException(nameof(element));

            if (!_data.TryGetValue(scene, out var container))
            {
                var containerData = new RoomContainerData
                (
                    new HashSet<object> { element }
                );

                _data[scene] = containerData;
                
                return true;
            }
            
            if (container.HasSameTypeObject<T>()) return false;
            
            container.Objects.Add(element);

            return true;
        }

        /// <summary>
        /// Removes an object of type T from the specified room's container.
        /// </summary>
        /// <typeparam name="T">The type of object to remove.</typeparam>
        /// <param name="scene">The scene from which to remove the object.</param>
        /// <returns>True if the object was successfully removed, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the scene parameter is null.</exception>
        internal bool Remove<T>(Scene scene) where T : class
        {
            if (scene == default) throw new ArgumentNullException(nameof(scene));

            if (!_data.TryGetValue(scene, out var container)) return false;

            var obj = container.GetObjectOfSameType<T>();

            return container.Objects.Remove(obj);
        }

        /// <summary>
        /// Gets an object of type T from the specified room.
        /// </summary>
        /// <typeparam name="T">The type of the object to retrieve.</typeparam>
        /// <param name="scene">The scene from which to retrieve the object.</param>
        /// <returns>An object of type T if found; otherwise, null.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the scene is null.</exception>
        internal T Get<T>(Scene scene) where T : class
        {
            if (scene == default) throw new ArgumentNullException(nameof(scene));

            if (!_data.TryGetValue(scene, out var container)) return null;

            var obj = container.GetObjectOfSameType<T>();

            return obj as T;
        }
    }

    internal class ListenerContainer
    {
        /// <summary>
        /// Manages the room listeners for a given room.
        /// </summary>
        private class RoomListenersHandler
        {
            public readonly List<IRoomSceneListener> SceneListeners = new();

            /// <summary>
            /// Adds a listener to the room.
            /// </summary>
            /// <param name="listener">The listener to be added. Must implement the <see cref="IRoomListener"/> interface.</param>
            internal void AddListener(IRoomListener listener)
            {
                if (listener is IRoomSceneListener sceneListener)
                {
                    SceneListeners.Add(sceneListener);
                }
            }

            /// <summary>
            /// Removes a listener from the room.
            /// </summary>
            /// <param name="listener">The listener to be removed. Must implement the <see cref="IRoomListener"/> interface.</param>
            internal void RemoveListener(IRoomListener listener)
            {
                if (listener is IRoomSceneListener sceneListener)
                {
                    SceneListeners.Remove(sceneListener);
                }
            }
        }

        private readonly Dictionary<string, RoomListenersHandler> _listenerHandlers = new();

        /// <summary>
        /// Calls scene listeners that have subscribed to scene changes in a specified room.
        /// </summary>
        /// <param name="scene">The scene to notify the listeners about.</param>
        /// <param name="roomName">The name of the room that has changed.</param>
        internal void CallSceneListeners(Scene scene, string roomName)
        {
            if (!HasRoom(roomName)) return;

            var listeners = _listenerHandlers[roomName].SceneListeners;
            
            listeners.ForEach(listener => listener.OnRoomSceneChanged(scene));
        }

        internal void RemoveRoomListenerHandlers(string roomName)
        {
            if (!HasRoom(roomName)) return;

            _listenerHandlers.Remove(roomName);
        }
        
        /// <summary>
        /// Registers a listener for a specified room.
        /// </summary>
        /// <param name="roomName">The name of the room to register the listener to.</param>
        /// <param name="roomListener">The listener object that will receive events from the room.</param>
        internal void RegisterListener(string roomName, IRoomListener roomListener)
        {
            if (!HasRoom(roomName))
            {
                var listenersHandler = new RoomListenersHandler();
                listenersHandler.AddListener(roomListener);
                
                _listenerHandlers.Add(roomName, listenersHandler);

                return;
            }
            
            _listenerHandlers[roomName].AddListener(roomListener);
        }

        /// <summary>
        /// Unregisters a listener from a specific room.
        /// </summary>
        /// <param name="roomName">The name of the room.</param>
        /// <param name="roomListener">The listener to be unregistered.</param>
        internal void UnRegisterListener(string roomName, IRoomListener roomListener)
        {
            if (!HasRoom(roomName)) return;
            
            _listenerHandlers[roomName].RemoveListener(roomListener);
        }

        /// <summary>
        /// Determines whether a specified room has registered listeners.
        /// </summary>
        /// <param name="roomName">The name of the room to check.</param>
        /// <returns>true if the room has registered listeners; otherwise, false.</returns>
        private bool HasRoom(string roomName)
        {
            return _listenerHandlers.ContainsKey(roomName);
        }
    }
}