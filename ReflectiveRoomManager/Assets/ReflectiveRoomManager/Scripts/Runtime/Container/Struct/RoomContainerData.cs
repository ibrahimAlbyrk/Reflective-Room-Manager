using System.Linq;
using System.Collections.Generic;

namespace REFLECTIVE.Runtime.Container.Data
{
    public readonly struct RoomContainerData
    {
        public readonly HashSet<object> Objects;
        
        public RoomContainerData(HashSet<object> objects)
        {
            Objects = objects;
        }
        
        public bool HasSameTypeObject<T>() => Objects.Any(obj => obj.GetType() == typeof(T));

        public object GetObjectOfSameType<T>() => Objects.FirstOrDefault(obj => obj.GetType() == typeof(T));
    }
}