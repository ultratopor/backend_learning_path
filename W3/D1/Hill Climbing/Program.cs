using System.Diagnostics;

namespace Hill_Climbing
{
    internal class Program
    {
        static void Main(string[] args)
        {
            for(int i=0; i<50; i++)
            {
                Task.Run(() => Thread.Sleep(10000));
            }

            ThreadPool.GetMinThreads(out int minWorkerThreads, out _);
            ThreadPool.GetMaxThreads(out int maxWorkerThreads, out _);
            Console.WriteLine($"Мин. потоков в пуле: {minWorkerThreads}, Макс. потоков: {maxWorkerThreads}\n");
            Console.WriteLine("Время      | Pool Threads | Process Threads");
            Console.WriteLine("------------------------------------------");

            for (int i = 0; i < 30; i++)
            {
                Thread.Sleep(1000);

                // Получаем количество СВОБОДНЫХ потоков
                ThreadPool.GetAvailableThreads(out int availableWorkerThreads, out _);

                // Вычисляем количество ЗАНЯТЫХ потоков в пуле
                int currentPoolThreads = maxWorkerThreads - availableWorkerThreads;

                // Получаем общее количество потоков в процессе для сверки
                int totalProcessThreads = Process.GetCurrentProcess().Threads.Count;

                Console.WriteLine($"{DateTime.Now:HH:mm:ss}   | {currentPoolThreads,-12} | {totalProcessThreads,-15}");
            }

            Console.ReadKey();
        }
    }
}
