using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.Container
{
    using Data;
    
    public static class RoomContainer
    {
        private static readonly Dictionary<Scene, RoomContainerData> _data = new();

        public static bool Add<T>(Scene scene, T element) where T : class
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

        public static bool Remove<T>(Scene scene) where T : class
        {
            if (scene == default) throw new ArgumentNullException(nameof(scene));
            
            if (!_data.TryGetValue(scene, out var container)) return false;

            var obj = container.GetObjectOfSameType<T>();

            return container.Objects.Remove(obj);
        }

        public static T Get<T>(Scene scene) where T : class
        {
            if (scene == default) throw new ArgumentNullException(nameof(scene));
            
            if (!_data.TryGetValue(scene, out var container)) return null;

            var obj = container.GetObjectOfSameType<T>();

            return obj as T;
        }
    }
}