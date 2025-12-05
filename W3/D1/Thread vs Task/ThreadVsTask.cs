using BenchmarkDotNet.Attributes;
using System.Threading;
using System.Threading.Tasks;

[MemoryDiagnoser]
public class ThreadVsTask
{
    [Params(100, 1000)]
    public int Count;

    [Benchmark(Baseline = true)]
    public void ManualThreads()
    {
        Thread[] threads = new Thread[Count];
        for (int i = 0; i < Count; i++)
        {
            threads[i] = new Thread(() =>
            {
                // Имитация крошечной работы
                int a = 1 + 1;
            });
            threads[i].Start();
        }

        foreach (var t in threads) t.Join();
    }

    [Benchmark]
    public void PoolTasks()
    {
        Task[] tasks = new Task[Count];
        for (int i = 0; i < Count; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                int a = 1 + 1;
            });
        }
        Task.WaitAll(tasks);
    }
}
