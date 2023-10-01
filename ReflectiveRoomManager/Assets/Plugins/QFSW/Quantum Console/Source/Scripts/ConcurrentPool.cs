using System.Collections.Concurrent;

namespace QFSW.QC
{
    public class ConcurrentPool<T> where T : class, new()
    {
        private readonly ConcurrentStack<T> _objs;

        public ConcurrentPool()
        {
            _objs = new ConcurrentStack<T>();
        }

        public ConcurrentPool(int objCount)
        {
            _objs = new ConcurrentStack<T>();
            for (int i = 0; i < objCount; i++)
            {
                _objs.Push(new T());
            }
        }

        public T GetObject()
        {
            if (_objs.TryPop(out T obj))
            {
                return obj;
            }
            else
            {
                return new T();
            }
        }

        public void Release(T obj)
        {
            _objs.Push(obj);
        }
    }
}