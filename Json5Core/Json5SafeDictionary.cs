using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Json5Core
{
    internal class ReferenceEqualityComparer : IEqualityComparer, IEqualityComparer<object>
    {
        public static ReferenceEqualityComparer Default { get; } = new ReferenceEqualityComparer();
        
        public new bool Equals(object x, object y) => x.Equals(y);
        public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj); 
    }

    public sealed class Json5SafeDictionary<TKey, TValue>
    {
        private readonly object _Padlock = new object();
        private readonly Dictionary<TKey, TValue> _Dictionary;

        public Json5SafeDictionary(int capacity)
        {
            _Dictionary = new Dictionary<TKey, TValue>(capacity);
        }

        public Json5SafeDictionary()
        {
            _Dictionary = new Dictionary<TKey, TValue>();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (_Padlock)
                return _Dictionary.TryGetValue(key, out value);
        }

        public int Count()
        {
            lock (_Padlock) return _Dictionary.Count;
        }

        public TValue this[TKey key]
        {
            get
            {
                lock (_Padlock)
                    return _Dictionary[key];
            }
            set
            {
                lock (_Padlock)
                    _Dictionary[key] = value;
            }
        }

        public void Add(TKey key, TValue value)
        {
            lock (_Padlock)
            {
                _Dictionary.TryAdd(key, value);
            }
        }
    }
}
