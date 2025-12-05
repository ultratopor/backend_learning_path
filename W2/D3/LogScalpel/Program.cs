using BenchmarkDotNet.Running;

namespace LogScalpel
{
    internal class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<LogParserBenchmarks>();
        }
    }
}
