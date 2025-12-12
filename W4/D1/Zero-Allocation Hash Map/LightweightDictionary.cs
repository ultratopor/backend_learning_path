using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace ZeroAllocation_Hash_Map
{
    
    internal class LightweightDictionary<K, V>
    {

        private Entry[] _entries;

        private int[] _buckets;
        private int _index;

        private struct Entry
        {
            public int HashCode;
            public int Next;
            public K Key;
            public V Value;
        }
        
        public void Add(K key, V value)
        {
            if(key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var index = (key.GetHashCode() & 0x7ffffffff) % _buckets.Length;

            if (_buckets[index] == -1)
            {
                _buckets[index] = _index;

                _entries[_index] = new Entry
                {
                    HashCode = key.GetHashCode(),
                    Next = -1,
                    Key = key,
                    Value = value
                };
            }
            else
            {
                _entries[_index] = new Entry
                {
                    HashCode = key.GetHashCode(),
                    Next = _buckets[index],
                    Key = key,
                    Value = value
                };
                _buckets[index] = _index;
            }
            _index++;
        }

        public bool TryGetValue(K key, out V value)
        {
            if(key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            var index = (key.GetHashCode() & 0x7ffffffff) % _buckets.Length;
            var entryIndex = _buckets[index];
            while (entryIndex != -1)
            {
                var entry = _entries[entryIndex];
                if (entry.HashCode == key.GetHashCode() && EqualityComparer<K>.Default.Equals(entry.Key, key))
                {
                    value = entry.Value;
                    return true;
                }
                entryIndex = entry.Next;
            }
            value = default(V);
            return false;
        }
    }
}
