using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using System;

namespace String_Wars
{
    internal class Program
    {
        
        static void Main(string[] args)
        {
            // Запуск всех бенчмарков в классе StringCreationBenchmarks
            var summary = BenchmarkRunner.Run<StringAllocations>();

            
        }
    }
}
