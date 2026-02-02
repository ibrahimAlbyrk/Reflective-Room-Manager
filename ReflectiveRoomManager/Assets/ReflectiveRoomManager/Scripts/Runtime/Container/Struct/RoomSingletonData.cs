using System;
using System.Collections.Generic;

namespace REFLECTIVE.Runtime.Container.Data
{
    public readonly struct RoomSingletonData
    {
        private readonly Dictionary<Type, object> _objects;

        public IReadOnlyCollection<object> Objects => _objects.Values;

        public RoomSingletonData(Dictionary<Type, object> objects)
        {
            _objects = objects;
        }

        public bool HasSameTypeObject<T>() => _objects.ContainsKey(typeof(T));

        public object GetObjectOfSameType<T>() =>
            _objects.TryGetValue(typeof(T), out var obj) ? obj : null;

        public void Add<T>(T element) => _objects[typeof(T)] = element;

        public bool Remove<T>() => _objects.Remove(typeof(T));
    }
}
