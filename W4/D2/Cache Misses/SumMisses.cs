using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;
using Microsoft.VSDiagnostics;

namespace Cache_Misses
{
    [MemoryDiagnoser]
    public class SumMisses
    {
        private List<int> list = new List<int>(1000000);

        private LinkedList<int> linkedList = new LinkedList<int>();

        public SumMisses()
        {
            for(int i = 0; i < list.Capacity; i++)
            {
                list[i] = i;
                linkedList.AddLast(i);
            }
        }

        [Benchmark]
        public long SumList()
        {
            long sum = 0;
            for(int i = 0; i < list.Count; i++)
            {
                sum += list[i];
            }
            return sum;
        }

        [Benchmark]
        public long SumLinkedList()
        {
            long sum = 0;
            foreach(var item in linkedList)
            {
                sum += item;
            }
            return sum;
        }
    }
}
