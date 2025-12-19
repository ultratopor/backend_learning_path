using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Collections.Generic;
using System.Linq;

namespace Cache_Misses
{
    [MemoryDiagnoser]
    public class CacheMissBenchmarks
    {
        [Params(1_000_000)]
        public int Size { get; set; }

        private List<int> _list;
        private LinkedList<int> _linkedList;

        [GlobalSetup]
        public void Setup()
        {
            _list = Enumerable.Range(0, Size).ToList();
            _linkedList = new LinkedList<int>(_list);
        }

        [Benchmark(Baseline = true)]
        public long SumList()
        {
            long sum = 0;
            foreach (var item in _list)
            {
                sum += item;
            }
            return sum;
        }

        [Benchmark]
        public long SumLinkedList()
        {
            long sum = 0;
            foreach (var item in _linkedList)
            {
                sum += item;
            }
            return sum;
        }
    }
}
