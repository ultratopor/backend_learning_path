using BenchmarkDotNet.Running;
using System;

namespace Cache_Misses
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //BenchmarkRunner.Run<CacheMissBenchmarks>();
            //BenchmarkRunner.Run<CacheMissBenchmarks>();
            BenchmarkRunner.Run<ParsingBenchmarks>();
        }
    }
}
