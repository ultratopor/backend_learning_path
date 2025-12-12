namespace SmartDownloader
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var urls = Enumerable.Range(1, 25).Select(i => $"http://fake.url.com/file{i}.dat").ToList();

            var semaphore = new SemaphoreSlim(3); // ограничение в три загрузки

            var cts = new CancellationTokenSource();

            Console.WriteLine($"Запуск загрузки {urls.Count} файлов с ограничением в 3 одновременных потока.");
            Console.WriteLine("Отмена произойдет через 2 секунды...\n");

            cts.CancelAfter(TimeSpan.FromSeconds(2));

            var downloadTasks = urls.Select(url => DownloadAsync(url, semaphore, cts.Token)).ToArray();

            try
            {
                await Task.WhenAll(downloadTasks);
            }
            catch(OperationCanceledException)
            {
                Console.WriteLine("\nОперация была отменена до завершения всех задач.");
            }

            Console.WriteLine("Все задачи завершены");
            Console.ReadKey();
        }

        private static async Task DownloadAsync(string url, SemaphoreSlim semaphore, CancellationToken cancellationToken)
        {
            var taskId = url.Split('/').Last().Replace(".dat", "");

            try
            {
                Console.WriteLine($"[Start] Task {taskId}");

                await semaphore.WaitAsync(cancellationToken);

                try
                {
                    var randomDelay = Random.Shared.Next(1, 5); // имитация загрузки 4 секунды
                    await Task.Delay(TimeSpan.FromSeconds(randomDelay), cancellationToken);

                    Console.WriteLine($"[End] Task {taskId}");
                }
                catch(OperationCanceledException)
                {
                    Console.WriteLine($"[Cancel] Task {taskId} (during download");
                }
                finally
                {
                    semaphore.Release();
                }
            }
            catch(OperationCanceledException)
            {
                Console.WriteLine($"[Cancel] Task {taskId} (while waiting)");
            }
        }
    }
}
