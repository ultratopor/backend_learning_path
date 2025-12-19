using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace InMemory_Data_Grid
{
    internal readonly struct CacheItem(byte[] data, long expirationTicks = 0)
    {
        public readonly byte[] Data = data;
        public readonly long ExpirationTicks = expirationTicks;
    }

    internal class MiniRedisService : IDisposable
    {
        public readonly ConcurrentDictionary<string, CacheItem> Storage = new();
        public readonly PriorityQueue<(long ExpirationTicks, string Key), long> ExpirationQueue = new();
        public readonly object ExpirationLock = new();
        private readonly Task _cleanupTask;
        private readonly CancellationTokenSource _cts =  new();
        private readonly FrozenDictionary<string, ICommand> _commands;
        public MiniRedisService()
        {
            var commandMap = new Dictionary<string, ICommand>(StringComparer.OrdinalIgnoreCase)
            {
                { "GET", new GetCommand(this) },
                { "SET", new SetCommand(this) },
                { "DEL", new DelCommand(this) },
            };

            _commands = commandMap.ToFrozenDictionary();

            _cleanupTask = Task.Run(ExpirationLoop);
        }

        private async Task ExpirationLoop()
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                (long expirationTicks, string key) keyValuePair;
                bool hasItem = false;

                lock (ExpirationLock) // <-- Блокируем
                {
                    if (ExpirationQueue.Count == 0)
                    {
                        // Выходим из блока, чтобы ждать не под замком
                        hasItem = false;
                    }
                    else
                    {
                        keyValuePair = ExpirationQueue.Peek();
                        var nowTicks = DateTime.UtcNow.Ticks;

                        if (keyValuePair.expirationTicks > nowTicks)
                        {
                            // Вычисляем задержку и выходим из-под замка
                            var delay = TimeSpan.FromTicks(keyValuePair.expirationTicks - nowTicks);
                            await Task.Delay(delay, _cts.Token);
                            continue; // Вернемся в начало цикла и снова проверим под замком
                        }

                        // Время пришло, извлекаем элемент
                        ExpirationQueue.Dequeue();
                        hasItem = true;
                    }
                }

                if (!hasItem)
                {
                    // Если в очереди было пусто, ждем немного
                    await Task.Delay(1000, _cts.Token);
                    continue;
                }

                // Теперь у нас есть keyValuePair, и работаем с ним вне замка
                if (Storage.TryGetValue(keyValuePair.key, out var existingItem))
                {
                    if (existingItem.ExpirationTicks == keyValuePair.expirationTicks)
                    {
                        _context.Storage.TryRemove(keyValuePair.Key, out _);
                    }
                }
            }
        }

        public async Task HandleClientAsync(Socket client)
        {
            byte[] buffer = ArrayPool<byte>.Shared.Rent(8192);
            try
            {
                while(true)
                {
                    int bytesRead = await client.ReceiveAsync(buffer, SocketFlags.None);
                    if (bytesRead == 0) break; // Клиент отключился
                    var receivedSpan = new ReadOnlySpan<byte>(buffer, 0, bytesRead);
                    var response = await ProcessRequestAsync(receivedSpan);
                    await client.SendAsync(response, SocketFlags.None);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
                client.Close();
            }
        }

        public async Task<byte[]> ProcessRequestAsync(ReadOnlySpan<byte> requestSpan)
        {
            // 1. Находим конец имени команды (первый пробел)
            int firstSpaceIndex = requestSpan.IndexOf((byte)' ');
            if(firstSpaceIndex == -1) return Error("Invalid command");

            // 2. Извлекаем имя команды и аргументы
            var commandNameSpan = requestSpan.Slice(0, firstSpaceIndex);
            var argumentsSpan = requestSpan.Slice(firstSpaceIndex + 1);

            // 3. Преобразуем имя команды в строку для поискав словаре
            string commandName = Encoding.UTF8.GetString(commandNameSpan);

            // 4. Ищем команду в словаре и выполняем её
            if (_commands.TryGetValue(commandName, out var command))
            {
                return await command.ExecuteAsync(argumentsSpan, this);
            }
            else
            {
                return Error("Unknown command");
            }
        }

        public static byte[] Error(string message)
        {
            return Encoding.UTF8.GetBytes($"-ERR {message}\r\n");
        }

        public static List<ReadOnlyMemory<byte>> SplitArguments(ReadOnlySpan<byte> span)
        {
            var parts = new List<ReadOnlyMemory<byte>>();
            int start = 0;
            while (true)
            {
                int index = span.IndexOf((byte)' ');
                if (index == -1)
                {
                    // Последний аргумент
                    parts.Add(span.ToArray().AsMemory());
                    break;
                }
                parts.Add(span.ToArray().AsMemory(start, index - start));
                start = index + 1;
                span = span[start..];
            }
            return parts;
        }

        public static long ParseLong(ReadOnlySpan<byte> span)
        {
            long result = 0;
            foreach (var b in span)
            {
                if (b < (byte)'0' || b > (byte)'9') throw new FormatException("Invalid number format");
                result = result * 10 + (b - (byte)'0');
            }
            return result;
        }

        public void Dispose()
        {
            _cts.Cancel();
            try
            {
                _cleanupTask?.Wait();
            }
            catch (AggregateException) 
            { 
            }
            finally
            {
                _cts.Dispose();

            }
        }
    }

    internal interface ICommand
    {
        Task<byte[]> ExecuteAsync(ReadOnlySpan<byte> arguments, MiniRedisService context);
    }

    internal class GetCommand(MiniRedisService context) : ICommand
    {
        private readonly MiniRedisService _context = context;
        public Task<byte[]> ExecuteAsync(ReadOnlySpan<byte> arguments, MiniRedisService context)
        {
            int firstSpaceIndex = arguments.IndexOf((byte)' ');

            var keySpan = firstSpaceIndex == -1 ? arguments : arguments.Slice(0, firstSpaceIndex);

            string key = Encoding.UTF8.GetString(keySpan);

            if (context.Storage.TryGetValue(key, out var cacheItem))
            {
                var data = cacheItem.Data;
                // "$" + длина + "\r\n" + данные + "\r\n"
                var responseString = $"${data.Length}\r\n{Encoding.UTF8.GetString(data)}\r\n";

                return Task.FromResult(Encoding.UTF8.GetBytes(responseString));
            }
            else
            {
                return Task.FromResult(Encoding.UTF8.GetBytes("$-1\r\n"));
            }
        }
    }

    internal class SetCommand(MiniRedisService context) : ICommand
    {
        private readonly MiniRedisService _context = context;
        public Task<byte[]> ExecuteAsync(ReadOnlySpan<byte> arguments, MiniRedisService context)
        {
            // Парсинг: SET key value [PX ms] [EX s]
            var parts = MiniRedisService.SplitArguments(arguments);

            if (parts.Count < 2) return Task.FromResult(MiniRedisService.Error("ERR wrong number of arguments"));

            string key = Encoding.UTF8.GetString(parts[0].Span);
            byte[] value = parts[1].ToArray(); 

            // Создаем и сохраняем элемент
            var cacheItem = new CacheItem(value);
            context.Storage.AddOrUpdate(key, cacheItem, (k, v) => cacheItem);

            long? expirationTicks = null;
            // Ищем опции TTL
            for (int i = 2; i < parts.Count; i += 2)
            {
                var optionSpan = parts[i].Span;
                if (optionSpan.SequenceEqual("PX"u8))
                {
                    long ms = MiniRedisService.ParseLong(parts[i+1].Span);
                    expirationTicks = DateTime.UtcNow.AddMilliseconds(ms).Ticks;
                }
                else if (optionSpan.SequenceEqual("EX"u8))
                {
                    long seconds = MiniRedisService.ParseLong(parts[i + 1].Span);
                    expirationTicks = DateTime.UtcNow.AddSeconds(seconds).Ticks;
                }
            }

            // Если TTL указан, добавляем в очередь экспирации
            if (expirationTicks.HasValue)
            {
                context.ExpirationQueue.Enqueue((expirationTicks.Value, key));
            }

            return Task.FromResult(Encoding.UTF8.GetBytes("+OK\r\n")); 
        }
    }

    internal class DelCommand(MiniRedisService context) : ICommand
    {
        private readonly MiniRedisService _context = context;
        public Task<byte[]> ExecuteAsync(ReadOnlySpan<byte> arguments, MiniRedisService context)
        {
            if (arguments.IsEmpty) return Task.FromResult(MiniRedisService.Error("ERR wrong number of arguments"));

            var parts = MiniRedisService.SplitArguments(arguments);
            int deletedCount = 0;

            foreach (var part in parts)
            {
                string key = Encoding.UTF8.GetString(part.Span);
                if (context.Storage.TryRemove(key, out _))
                {
                    deletedCount++;
                }
            }

            var response = $":{deletedCount}\r\n";
            return Task.FromResult(Encoding.UTF8.GetBytes(response));
        }
    }
}
