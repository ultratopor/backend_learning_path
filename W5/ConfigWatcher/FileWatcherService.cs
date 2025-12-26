using Microsoft.Extensions.Hosting;
using Polly;
using System.Reactive.Linq;
using System.Text.Json;

namespace ConfigWatcher
{
    internal class FileWatcherService(string fullPathToWatch) : IHostedService, IDisposable
    {
        public event Action<string>? OnConfigChanged;
        private  FileSystemWatcher? _fileWatcher;
        private  IDisposable? _subscription;
        private readonly string _fullPath = fullPathToWatch;

        public Task StartAsync(CancellationToken token)
        {
            var directory = Path.GetDirectoryName(_fullPath);
            var fileName = Path.GetFileName(_fullPath);

            if (string.IsNullOrEmpty(directory)) directory = AppContext.BaseDirectory;

            _fileWatcher = new(directory, fileName)
            {
                EnableRaisingEvents = true,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime
            };

            var changed = Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                h => _fileWatcher.Changed += h, h => _fileWatcher.Changed -= h);

            var created = Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                h => _fileWatcher.Created += h, h => _fileWatcher.Created -= h);

            var renamed = Observable.FromEventPattern<RenamedEventHandler, RenamedEventArgs>(
                h => _fileWatcher.Renamed += h, h => _fileWatcher.Renamed -= h);

            _subscription = Observable.Merge(
                changed.Select(_=System.Reactive.Unit.Default), 
                created.Select(_= System.Reactive.Unit.Default), 
                renamed.Select.(_= System.Reactive.Unit.Default)
                .Throttle(TimeSpan.FromMilliseconds(500)) // Debounce rapid events
                .SelectMany(async _ =>
                {
                    // Асинхронное чтение без блокировки потока
                    // Polly тоже должен быть Async (WaitAndRetryAsync)
                    return await ReadConfigAsyncWithRetry(_fullPath);
                })
                .Subscribe(content =>
                {
                    // Тут уже только легкая логика обновления
                    if (content != null) OnConfigChanged?.Invoke(content);
                },
                ex => Console.WriteLine("FATAL: Subscription died")));

            return Task.CompletedTask;
        }

        private async static Task<string> ReadConfigAsyncWithRetry(string path)
        {
            const int retryCount = 3;
            var jitterer = Random.Shared;

            var retryPolicy = Policy.Handle<IOException>()
                .WaitAndRetry(retryCount, retryAttempt =>
                {
                    var timeToWait = TimeSpan.FromMilliseconds(100 * Math.Pow(2, retryAttempt));
                    var randomJitter = TimeSpan.FromMilliseconds(jitterer.Next(-10, 10));

                    return timeToWait + randomJitter;
                },
                (exception, timeSpan, attempt, context) =>
                {
                    Console.WriteLine($"Retry {attempt} due to {exception.GetType().Name}. Waiting {timeSpan.TotalMilliseconds}ms");
                });

            return await retryPolicy.Execute(() => File.ReadAllTextAsync(path));
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Dispose();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _subscription?.Dispose();

            _fileWatcher?.EnableRaisingEvents = false;
            _fileWatcher?.Dispose();

            Console.WriteLine("Service disposed");
        }
    }
}
