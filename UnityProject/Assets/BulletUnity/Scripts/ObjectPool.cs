using System.Collections.Generic;

namespace BulletUnity
{
    public class ObjectPool<T>
    {
        private readonly int _growthStep;
        private readonly Stack<T> _pool;

        public ObjectPool(int capacity)
        {
            _growthStep = capacity;
            _pool = new Stack<T>();
        }

        public T Take()
        {
            return _pool.Pop();
        }

        public void Return(T @object)
        {
            _pool.Push(@object);
        }
    }
}