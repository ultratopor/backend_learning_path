using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace ZeroAllocLRUCache
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
        }
    }

    public class HighPerformanceLRU<TKey, TValue>
    {
        private readonly LruSegment<TKey, TValue>[] _segments;
        //private readonly int _segmentCapacity;
        private readonly int _segmentsSize;

        public HighPerformanceLRU(int segmentsSize, int segmentCapacity)
        {
            if (segmentsSize <= 0) throw new ArgumentOutOfRangeException(nameof(segmentsSize));
            if (segmentCapacity <= 0) throw new ArgumentOutOfRangeException(nameof(segmentCapacity));
            _segmentsSize = segmentsSize;
            //_segmentCapacity = segmentCapacity;
            _segments = new LruSegment<TKey, TValue>[segmentsSize];
            for (int i = 0; i < segmentsSize; i++)
            {
                _segments[i] = new LruSegment<TKey, TValue>(segmentCapacity);
            }
        }

        public bool TryGet(TKey key, out TValue value)
        {
            int segmentIndex = GetSegmentIndex(key);
            return _segments[segmentIndex].TryGet(key, out value);
        }


        private int GetSegmentIndex(TKey key)
        {
            int hash = key.GetHashCode();
            return (hash & 0x7FFFFFFF) % _segmentsSize;
        }

        public void Put(TKey key, TValue value)
        {
            int segmentIndex = GetSegmentIndex(key);
            _segments[segmentIndex].Put(key, value);
        }
    }

    public class LruSegment<TKey, TValue>
    {
        public struct LruNode
        {
            public TKey Key;       
            public TValue Value;

            public int PrevIndex;
            public int NextIndex;
        }

        private readonly LruNode[] _nodes;
        private readonly Dictionary<TKey, int> _keyToIndex;
        private readonly ReaderWriterLockSlim _lock = new();
        private ConcurrentStack<int> _freeIndices = new();

        private int _headIndex = -1;
        private int _tailIndex = -1;
        //private int _freeIndexHead = 0;
        private int _count = 0;

        public LruSegment(int capacity)
        {
            //if(capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            _nodes = new LruNode[capacity];
            _keyToIndex = new Dictionary<TKey, int>(capacity);


        }

        public bool TryGet(TKey key, out TValue value)
        {
            _lock.EnterWriteLock();
            try
            {
                if (!_keyToIndex.TryGetValue(key, out int index))
                {
                    value = default!;
                    return false;
                }
                
                if(index != _headIndex)
                {
                    MoveToHead(index);
                }

                value = _nodes[index].Value;
                return true;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        private void MoveToHead(int nodeIndex)
        {
            // 1. вырезаем узел из текущего контекста
            ref LruNode node = ref _nodes[nodeIndex];
            if (node.PrevIndex != -1)
            {
                _nodes[node.PrevIndex].NextIndex = node.NextIndex;
            }
            else
            {
                _headIndex = node.NextIndex;
            }

            if (node.NextIndex != -1)
            {
                _nodes[node.NextIndex].PrevIndex = node.PrevIndex;
            }
            else
            {
                _tailIndex = node.PrevIndex;
            }

            // 2. вставляем узел в голову
            node.PrevIndex = -1;
            node.NextIndex = _headIndex;
            _nodes[nodeIndex] = node;

            if (_headIndex != -1)
            {
                _nodes[_headIndex].PrevIndex = nodeIndex;
            }

            _headIndex = nodeIndex;

            if (_tailIndex == -1)
            {
                _tailIndex = nodeIndex;
            }
        }

        public void Put(TKey key, TValue value)
        {
            _lock.EnterWriteLock();
            try
            {
                if(_keyToIndex.TryGetValue(key, out int existingNodeIndex))
                {
                    _nodes[existingNodeIndex].Value = value;
                    MoveToHead(existingNodeIndex);
                    return;
                }

                if(_count == _nodes.Length)
                {
                    RemoveTail();
                }

                if(!_freeIndices.TryPop(out int newNodeIndex))
                {
                    throw new InvalidOperationException("Free indices stack is empty despite available capacity.");
                }

                var newNode = new LruNode
                {
                    Key = key,
                    Value = value,
                    PrevIndex = -1,
                    NextIndex = _headIndex
                };
                _nodes[newNodeIndex] = newNode;

                _keyToIndex[key] = newNodeIndex;

                if(_headIndex != -1)
                {
                    _nodes[_headIndex].PrevIndex = newNodeIndex;
                }
                _headIndex = newNodeIndex;

                if(_tailIndex == -1)
                {
                    _tailIndex = newNodeIndex;
                }

                _count++;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        private void RemoveTail()
        {
            if (_tailIndex == -1) return;

            var tailNode = _nodes[_tailIndex];
            _keyToIndex.Remove(tailNode.Key);
            tailNode.Key = default!;
            tailNode.Value = default!;

            _freeIndices.Push(_tailIndex);

            if (tailNode.PrevIndex != -1)
            {
                _nodes[tailNode.PrevIndex].NextIndex = -1;
            }
            else
            {
                _headIndex = -1;
            }

            _tailIndex = tailNode.PrevIndex;
            _count--;
        }
    }
}
