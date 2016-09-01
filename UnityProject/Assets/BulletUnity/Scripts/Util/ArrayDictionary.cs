using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BulletUnity {

    public class ArrayDictionary<TKey, TValue> : IDictionary<TKey, TValue> {

        private readonly IEqualityComparer<TValue> _comparer; 
        private readonly Func<TKey, int> _keyToIndex;
        private readonly Func<int, TKey> _indexToKey;

        private int _count;
        private readonly bool[] _isSet;
        private readonly TValue[] _dict;

        public ArrayDictionary(int size, IDictionary<TKey, TValue> existingDict = null,
            IEqualityComparer<TValue> comparer = null) {

            _count = 0;
            _comparer = comparer ?? EqualityComparer<TValue>.Default;
            _isSet = new bool[size];
            _dict = new TValue[size];

            var intConverter = TypeConversion.IntConverter<TKey>();
            _keyToIndex = intConverter.ToInt;
            _indexToKey = intConverter.FromInt;

            if (existingDict != null) {
                for (int i = 0; i < existingDict.Count; i++) {
                    var key = _indexToKey(i);
                    TValue value;
                    if (existingDict.TryGetValue(key, out value)) {
                        Add(key, value);    
                    }
                }
            }
        }

        public ArrayDictionary(Func<TKey, int> keyToIndex, Func<int, TKey> indexToKey, int size, 
            IDictionary<TKey, TValue> existingDict = null,
            IEqualityComparer<TValue> comparer = null) {

            _comparer = comparer ?? EqualityComparer<TValue>.Default;
            _keyToIndex = keyToIndex;
            _indexToKey = indexToKey;
            _isSet = new bool[size];
            _dict = new TValue[size];

            if (existingDict != null) {
                for (int i = 0; i < existingDict.Count; i++) {
                    var key = _indexToKey(i);
                    TValue value;
                    if (existingDict.TryGetValue(key, out value)) {
                        Add(key, value);    
                    }
                }
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
            for (int i = 0; i < _dict.Length; i++) {
                if (_isSet[i]) {
                    yield return new KeyValuePair<TKey, TValue>(_indexToKey(i), _dict[i]);    
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<TKey, TValue> item) {
            Add(item.Key, item.Value);
        }

        public void Clear() {
            for (int i = 0; i < _dict.Length; i++) {
                _dict[i] = default(TValue);
                _isSet[i] = false;
            }
            _count = 0;
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) {
            var index = _keyToIndex(item.Key);
            return index < _dict.Length && _isSet[index] && _comparer.Equals(_dict[index], item.Value);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
            if (array == null) {
                throw new ArgumentNullException("array");
            }
            if (arrayIndex < 0) {
                throw new ArgumentOutOfRangeException("arrayIndex");
            }
            if (array.Length - arrayIndex < _dict.Length) {
                throw new ArgumentException("array is too small to copy all elements of this dictionary");
            }

            for (int i = 0; i < _dict.Length; i++) {
                if (_isSet[i]) {
                    array[arrayIndex] = new KeyValuePair<TKey, TValue>(_indexToKey(i), _dict[i]);
                    arrayIndex++;
                }
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item) {
            if (Contains(item)) {
                return Remove(item.Key);
            }
            return false;
        }

        public int Count {
            get { return _count; }
        }

        public bool IsReadOnly {
            get { return false; }
        }

        public void Add(TKey key, TValue value) {
            var index = _keyToIndex(key);
            if (index < _dict.Length) {
                Remove(key);
                _dict[index] = value;
                _isSet[index] = true;
                _count++;
            } else {
                throw new IndexOutOfRangeException("index: " + index + " is bigger than: " + (_dict.Length - 1));
            }
        }

        public bool ContainsKey(TKey key) {
            var index = _keyToIndex(key);
            return index < _dict.Length && _isSet[index];
        }

        public bool Remove(TKey key) {
            var index = _keyToIndex(key);
            if (index < _dict.Length && _isSet[index]) {
                _dict[index] = default(TValue);
                _isSet[index] = false;
                _count--;
                return true;
            } 
            return false;
        }

        public bool TryGetValue(TKey key, out TValue value) {
            var index = _keyToIndex(key);
            if (index < _dict.Length && _isSet[index]) {
                value = _dict[index];
                return true;
            }
            value = default(TValue);
            return false;
        }

        public TValue this[TKey key] {
            get { return _dict[_keyToIndex(key)]; } 
            set { Add(key, value); }
        }

        public ICollection<TKey> Keys {
            get { throw new NotImplementedException(); }
        }

        public ICollection<TValue> Values {
            get { throw new NotImplementedException(); }
        }
    }
}
