using System;
using System.Collections.Generic;
using System.Linq;
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
            private readonly Dictionary<Type, List<IRoomListener>> listeners = new();
            
            public readonly List<IRoomSceneListener> SceneListeners = new();

            public List<T> GetListeners<T>() where T : IRoomListener
            {
                if (!listeners.TryGetValue(typeof(T), out var list))
                    return null;

                return list.Cast<T>().ToList();
            }

            public void AddListener<T>(T listener) where T : IRoomListener
            {
                if (!listeners.TryGetValue(typeof(T), out var list))
                {
                    list = new List<IRoomListener> { listener };
                    listeners[typeof(T)] = list;
                }
                
                list.Add(listener);
            }

            public void RemoveListener<T>(T listener) where T : IRoomListener
            {
                if (listeners.TryGetValue(typeof(T), out var list))
                {
                    list.Remove(listener);
                }
            }
        }

        private readonly Dictionary<string, RoomListenersHandler> _listenerHandlers = new();

        /// <summary>
        /// Calls the scene change listeners for a given room and scene.
        /// </summary>
        /// <param name="roomName">The name of the room where the scene change listeners are located.</param>
        /// <param name="scene">The scene that is being changed to.</param>
        internal void CallSceneChangeListeners(string roomName, Scene scene)
        {
            if (!HasRoom(roomName)) return;

            var listeners = GetListeners<IRoomSceneListener>(roomName);

            foreach (var listener in listeners)
            {
                listener.OnRoomSceneListener(scene);
            }
        }

        internal void RemoveRoomListenerHandlers(string roomName)
        {
            if (!HasRoom(roomName)) return;

            _listenerHandlers.Remove(roomName);
        }
        
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
        
        internal void UnRegisterListener(string roomName, IRoomListener roomListener)
        {
            if (!HasRoom(roomName)) return;
            
            _listenerHandlers[roomName].RemoveListener(roomListener);
        }

        private List<T> GetListeners<T>(string roomName) where T : IRoomListener
        {
            return _listenerHandlers[roomName].GetListeners<T>();
        }
        
        private bool HasRoom(string roomName)
        {
            return _listenerHandlers.ContainsKey(roomName);
        }
    }
}