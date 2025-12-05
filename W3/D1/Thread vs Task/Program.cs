using BenchmarkDotNet.Running;

namespace Thread_vs_Task
{
    internal class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<ThreadVsTask>();
        }
    }
}
