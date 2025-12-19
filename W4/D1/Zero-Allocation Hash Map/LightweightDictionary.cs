using System;
using System.Collections.Generic;

namespace ZeroAllocation_Hash_Map
{

    internal class LightweightDictionary<K, V>
    {

        private Entry[] _entries;

        private int[] _buckets;
        private int _index;
        private float _loadFactor = 0.75f;

        private struct Entry
        {
            public int HashCode;
            public int Next;
            public K Key;
            public V Value;
        }

        public LightweightDictionary(int capacity)
        {
            _entries = new Entry[capacity];
            _buckets = new int[capacity];
            for (int i = 0; i < _buckets.Length; i++)
            {
                _buckets[i] = -1;
            }
            _index = 0;
        }

        public void Add(K key, V value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            int hashCode = key.GetHashCode();
            int bucketIndex = (hashCode & 0x7FFFFFFF) % _buckets.Length;

            // 1. Поиск дубликата
            int entryIndex = FindEntry(bucketIndex, key, hashCode);
            if (FindEntry(bucketIndex, key, hashCode) != -1)
            {
                throw new ArgumentException("An item with the same key has already been added.");
            }

            // 2. Добавление (если дубликата нет)

            _entries[_index] = new Entry
            {
                HashCode = hashCode,
                Next = _buckets[bucketIndex],
                Key = key,
                Value = value
            };
            _buckets[bucketIndex] = _index;

            _index++;

            // 3. Проверка необходимости изменения размера
            if (_index >= _entries.Length * _loadFactor)
            {
                Resize();
            }
        }

        public bool TryGetValue(K key, out V value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            int hashCode = key.GetHashCode();
            var index = (hashCode & 0x7FFFFFFF) % _buckets.Length;
            var entryIndex = FindEntry(index, key, hashCode);

            if (entryIndex != -1)
            {
                value = _entries[entryIndex].Value;
                return true;
            }

            value = default;
            return false;
        }

        private int FindEntry(int bucketIndex, K key, int hashCode)
        {

            for (int entryIndex = _buckets[bucketIndex]; entryIndex != -1; entryIndex = _entries[entryIndex].Next)
            {
                if (_entries[entryIndex].HashCode == hashCode)
                {
                    if (EqualityComparer<K>.Default.Equals(_entries[entryIndex].Key, key))
                        return entryIndex;
                }
            }
            return -1;
        }

        private void Resize()
        {
            var newSize = GetNextPrime(_entries.Length * 2);
            var newBuckets = new int[newSize];
            for (int i = 0; i < newBuckets.Length; i++)
            {
                _buckets[i] = -1;
            }
            var newEntries = new Entry[newSize];

            for (int i = 0; i < _index; i++)
            {
                ref Entry oldEntry = ref _entries[i];
                var newBucketIndex = (oldEntry.HashCode & 0x7FFFFFFF) % newSize;
                newEntries[i] = oldEntry;
                newEntries[i].Next = newBuckets[newBucketIndex];
                newBuckets[newBucketIndex] = i;
            }

            _buckets = newBuckets;
            _entries = newEntries;
        }

        private static int GetNextPrime(int min)
        {
            if (min <= 2)
                return 2;

            int candidate = min;
            if (candidate % 2 == 0)
                candidate++;

            while (!IsPrime(candidate))
            {
                candidate += 2;
            }
            return candidate;
        }

        private static bool IsPrime(int number)
        {
            if (number < 2)
                return false;
            if (number == 2 || number == 3)
                return true;
            if (number % 2 == 0)
                return false;

            for (int i = 3; i * i <= number; i += 2)
            {
                if (number % i == 0)
                    return false;
            }
            return true;
        }
    }
}
