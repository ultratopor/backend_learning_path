using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DIYHashMap
{
    internal class SimpleIntStringMap
    {
        private int[] _buckets;
        private Entry[] _entries;
        private int _count;
        private int _resizeTreshold;

        private struct Entry
        {
            public int Key;
            public string Value;
            public int Next;
        }

        public SimpleIntStringMap(int capacity = 4)
        {
            _buckets = new int[capacity];
            _entries = new Entry[capacity];
            _resizeTreshold = (int)(capacity * 0.75);
            for (int i = 0; i < _buckets.Length; i++)
            {
                _buckets[i] = -1;
            }
        }

        public void Add(int key, string value)
        {
            if (_count >= _resizeTreshold)
            {
                Resize();
            }

            var index = (key.GetHashCode() & 0x7FFFFFFF) % _buckets.Length;

            for(int i = _buckets[index]; i != -1; i = _entries[i].Next)
            {
                if (_entries[i].Key == key)
                {
                    throw new ArgumentException("An item with the same key has already been added.");
                }
            }

            int newentryIndex = _count++;
            _entries[newentryIndex] = new Entry
            {
                Key = key,
                Value = value,
                Next = _buckets[index]
            };
            _buckets[index] = newentryIndex;
        }

        public bool TryGet(int key, out string value)
        {
            var index = (key.GetHashCode() & 0x7FFFFFFF) % _buckets.Length;

            for(int i = _buckets[index]; i != -1; i = _entries[i].Next)
            {
                if (_entries[i].Key == key)
                {
                    value = _entries[i].Value;
                    return true;
                }
            }
            value = null;
            return false;
        }

        private void Resize()
        {
            int newSize = _buckets.Length * 2;
            int[] newBuckets = new int[newSize];
            Entry[] newEntries = new Entry[newSize];
            for (int i = 0; i < newBuckets.Length; i++)
            {
                newBuckets[i] = -1;
            }

            for (int i = 0; i < _count; i++)
            {
                ref Entry oldEntry = ref _entries[i];

                // Пересчитываем индекс бакета для старого элемента, т.к. размер изменился
                int newBucketIndex = (oldEntry.Key.GetHashCode() & 0x7FFFFFFF) % newSize;

                // Копируем саму запись
                newEntries[i] = oldEntry;

                // Встраиваем запись в новую цепочку коллизий
                newEntries[i].Next = newBuckets[newBucketIndex];
                newBuckets[newBucketIndex] = i;
            }
            _buckets = newBuckets;
            _entries = newEntries;
            _resizeTreshold = (int)(newSize * 0.75);
        }
    }
}
