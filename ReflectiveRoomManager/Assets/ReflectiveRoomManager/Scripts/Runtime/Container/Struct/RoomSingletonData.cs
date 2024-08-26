using System.Linq;
using System.Collections.Generic;

namespace REFLECTIVE.Runtime.Container.Data
{
    /// <summary>
    /// Document: https://reflective-roommanager.gitbook.io/docs/user-manual/room-container/RoomSingletonData
    /// </summary>
    public readonly struct RoomSingletonData
    {
        public readonly HashSet<object> Objects;
        
        public RoomSingletonData(HashSet<object> objects)
        {
            Objects = objects;
        }
        
        public bool HasSameTypeObject<T>() => Objects.Any(obj => obj.GetType() == typeof(T));

        public object GetObjectOfSameType<T>() => Objects.FirstOrDefault(obj => obj.GetType() == typeof(T));
    }
}