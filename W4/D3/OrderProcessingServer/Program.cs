using System.Collections.Concurrent;
using System.Threading.Channels;

namespace OrderProcessingServer
{
    internal class Program
    {
        
        public static async Task Main()
        {
            var service = new OrderProcessingService(10000);
            var cts = new CancellationTokenSource();

            // 1. запуск потребителей
            const int workerCount = 4;
            var consumerTasks = new Task[workerCount];

            for(int i = 0; i < workerCount; i++)
            {
                int workerId = i + 1;
                consumerTasks[i] = Task.Run(() => service.RunConsumerAsync(workerId, cts.Token));
            }

            // 2. запуск производителя и ожидание завершения
            await service.RunProducerAsync(10_000);

            // 3. ожидание завершения потребителей
            Console.WriteLine("Ожидание завершения потребителей...");
            await Task.WhenAll(consumerTasks);

            Console.WriteLine("Система корректно остановлена. Все заказы обработаны.");
        }


    }

}
