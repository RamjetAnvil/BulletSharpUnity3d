using System.Collections.Generic;

namespace BulletUnity
{
    public class ObjectPool<T> where T : new()
    {
        private readonly int _growthStep;
        private readonly Stack<T> _pool;

        public ObjectPool(int capacity)
        {
            _growthStep = capacity;
            _pool = new Stack<T>();
            GrowPool();
        }

        public T Take()
        {
            if (_pool.Count == 0)
            {
                GrowPool();
            }
            return _pool.Pop();
        }

        public void Return(T @object)
        {
            _pool.Push(@object);
        }

        private void GrowPool()
        {
            for (int i = 0; i < _growthStep; i++)
            {
                _pool.Push(new T());
            }
        }
    }
}