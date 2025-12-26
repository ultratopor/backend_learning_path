using Microsoft.Extensions.Hosting;

namespace ConfigWatcher
{
    internal class GreetingService : IDisposable, IHostedService
    {
        private readonly FileWatcherService _service;

        public GreetingService(FileWatcherService service)
        {
            _service = service;
            service.OnConfigChanged += ConfigChanged;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("GreetingService is ready to receive updates...");
            // Просто возвращаем готовый Task, так как нам нечего делать при старте
            // Мы работаем реактивно (через ивент)
            return Task.CompletedTask;
        }  

        private void ConfigChanged(string newGreeting)
        {
            Console.WriteLine($" >>> GREETING UPDATED: {newGreeting} <<<");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Dispose();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _service.OnConfigChanged -= ConfigChanged;
        }
    }
}
