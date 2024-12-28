using System;
using System.Collections;
using System.Collections.Concurrent;
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
        private readonly ConcurrentDictionary<TKey, TValue> _dictionary;

        public Json5SafeDictionary(int capacity)
        {
            _dictionary = new ConcurrentDictionary<TKey, TValue>(Environment.ProcessorCount, capacity, EqualityComparer<TKey>.Default);
        }

        public Json5SafeDictionary()
        {
            _dictionary = new ConcurrentDictionary<TKey, TValue>();
        }

        public bool TryGetValue(TKey key, out TValue value)
            => _dictionary.TryGetValue(key, out value);

        public int Count => _dictionary.Count;

        public TValue this[TKey key]
        {
            get => _dictionary[key];
            set => _dictionary[key] = value;
        }

        public void Add(TKey key, TValue value)
            => _dictionary.TryAdd(key, value);
    }

}
