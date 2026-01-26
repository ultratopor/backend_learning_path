using BenchmarkDotNet.Running;
using Calendar_Service.Benchmarks;

namespace Calendar_Service.Benchmark;

internal class Program
{
    static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<HistoryBenchmark>();
    }
}
