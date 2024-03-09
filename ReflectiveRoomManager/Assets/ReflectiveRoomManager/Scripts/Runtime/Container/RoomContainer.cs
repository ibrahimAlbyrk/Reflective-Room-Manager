using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.Container
{
    using Data;
    using NETWORK.Room.Listeners;
    
    public static class RoomContainer
    {
        public static readonly SingletonContainer Singleton = new();

        public static readonly ListenerContainer Listener = new();
    }

    /// <summary>
    /// Provides functionality for managing and storing singleton objects associated with scenes.
    /// </summary>
    public class SingletonContainer
    {
        private readonly Dictionary<Scene, RoomSingletonData> _data = new();

        /// <summary>
        /// Adds an element of type T to the specified room.
        /// </summary>
        /// <param name="scene">The scene to add the element to.</param>
        /// <param name="element">The element to be added.</param>
        /// <typeparam name="T">The type of the element to be added.</typeparam>
        /// <returns>True if the element was successfully added; false if the scene already contains an element of the same type.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either the scene or the element is null.</exception>
        public bool Add<T>(Scene scene, T element) where T : class
        {
            if (scene == default) throw new ArgumentNullException(nameof(scene));
            if (element == null) throw new ArgumentNullException(nameof(element));

            if (!_data.TryGetValue(scene, out var container))
            {
                var containerData = new RoomSingletonData
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
        public bool Remove<T>(Scene scene) where T : class
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
        public T Get<T>(Scene scene) where T : class
        {
            if (scene == default) throw new ArgumentNullException(nameof(scene));

            if (!_data.TryGetValue(scene, out var container)) return null;

            var obj = container.GetObjectOfSameType<T>();

            return obj as T;
        }
    }

    /// <summary>
    /// Provides functionality to manage and invoke room listeners.
    /// </summary>
    public class ListenerContainer
    {
        /// <summary>
        /// Provides functionality to manage and invoke room listeners for a specific room.
        /// </summary>
        private class RoomListenersHandler
        {
            private readonly Dictionary<Type, List<IRoomListener>> listeners = new();

            /// <summary>
            /// Retrieves a list of listeners of type T for a specific room.
            /// </summary>
            /// <typeparam name="T">The type of the listeners to retrieve.</typeparam>
            /// <returns>The list of listeners of type T for the specified room. Returns null if there are no listeners of type T for the room.</returns>
            public List<T> GetListeners<T>()
            {
                if (!listeners.TryGetValue(typeof(T), out var list))
                    return null;

                return list.Cast<T>().ToList();
            }

            /// <summary>
            /// Adds a listener of type T to the specified room.
            /// </summary>
            /// <typeparam name="T">The type of the listener to be added.</typeparam>
            /// <param name="listener">The listener to be added.</param>
            public void AddListener<T>(IRoomListener listener)
            {
                if (!listeners.TryGetValue(typeof(T), out var list))
                {
                    list = new List<IRoomListener> { listener };
                    listeners[typeof(T)] = list;
                    return;
                }
                
                list.Add(listener);
            }

            /// <summary>
            /// Removes a listener of type T from the specified room.
            /// </summary>
            /// <typeparam name="T">The type of the listener to be removed.</typeparam>
            /// <param name="listener">The listener to be removed.</param>
            public void RemoveListener<T>(IRoomListener listener)
            {
                if (listeners.TryGetValue(typeof(T), out var list))
                {
                    list.Remove(listener);
                }
            }
        }

        /// <summary>
        /// The ListenerContainer class handles the registration, unregistration, and calling of room listeners.
        /// </summary>
        private readonly Dictionary<string, RoomListenersHandler> _listenerHandlers = new();

        /// <summary>
        /// Calls the scene change listeners for a specific room when the scene is changed.
        /// </summary>
        /// <param name="roomName">The name of the room.</param>
        /// <param name="scene">The new scene that has been changed to.</param>
        public void CallSceneChangeListeners(string roomName, Scene scene)
        {
            if (!HasRoom(roomName)) return;

            var listeners = GetListeners<IRoomSceneListener>(roomName);

            if (listeners == null) return;
            
            foreach (var listener in listeners)
            {
                listener?.OnRoomSceneChanged(scene);
            }
        }
        
        public void RemoveRoomListenerHandlers(string roomName)
        {
            if (!HasRoom(roomName)) return;

            _listenerHandlers.Remove(roomName);
        }

        /// <summary>
        /// Registers a listener of type T to the specified room.
        /// </summary>
        /// <param name="roomName">The name of the room to register the listener to.</param>
        /// <param name="listener">The listener to be registered.</param>
        /// <typeparam name="T">The type of the listener to be registered.</typeparam>
        public void RegisterListener<T>(string roomName, IRoomListener listener)
        {
            if (!HasRoom(roomName))
            {
                var listenersHandler = new RoomListenersHandler();
                listenersHandler.AddListener<T>(listener);
                
                _listenerHandlers.Add(roomName, listenersHandler);

                return;
            }
            
            _listenerHandlers[roomName].AddListener<T>(listener);
        }

        /// <summary>
        /// Unregisters a room listener of type T from the specified room.
        /// </summary>
        /// <typeparam name="T">The type of the room listener to unregister.</typeparam>
        /// <param name="roomName">The name of the room.</param>
        /// <param name="listener">The room listener to unregister.</param>
        public void UnRegisterListener<T>(string roomName, IRoomListener listener)
        {
            if (!HasRoom(roomName)) return;
            
            _listenerHandlers[roomName].RemoveListener<T>(listener);
        }
        
        private List<T> GetListeners<T>(string roomName) where T : IRoomListener
        {
            return _listenerHandlers[roomName].GetListeners<T>();
        }
        
        private bool HasRoom(string roomName)
        {
            if (_listenerHandlers == null) return false;
            
            return _listenerHandlers.ContainsKey(roomName);
        }
    }
}