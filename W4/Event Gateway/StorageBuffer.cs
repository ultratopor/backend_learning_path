using System.Collections.Frozen;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Event_Gateway
{
    public class StorageBuffer
    {
        private readonly Channel<SensorEvent> _channel;
        private readonly FrozenDictionary<int, string> _eventTypes;
        private readonly ILogger<StorageBuffer> _logger;
        private readonly CancellationTokenSource _shutdownCts = new();
        private readonly Task _processingTask;

        public ChannelWriter<SensorEvent> Writer => _channel.Writer;

        public StorageBuffer(FrozenDictionary<int, string> eventTypes, ILogger<StorageBuffer> logger)
        {
            _eventTypes = eventTypes;
            _logger = logger;
            // Создаем неограниченный канал, так как бэкпрессure управляется на входе в систему.
            _channel = Channel.CreateUnbounded<SensorEvent>();
            _processingTask = ProcessBatchesAsync(_shutdownCts.Token);
        }

        private async Task ProcessBatchesAsync(CancellationToken cancellationToken)
        {
            var batch = new List<SensorEvent>(100);
            // CTS для управления таймаутом сброса батча.
            var flushTimerCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Ждем либо появления данных в канале, либо срабатывания таймера.
                    await _channel.Reader.WaitToReadAsync(flushTimerCts.Token).ConfigureAwait(false);

                    // Данные появились! Сбрасываем таймер, чтобы отсчитать 500мс заново.
                    flushTimerCts.CancelAfter(TimeSpan.FromMilliseconds(500));

                    // Вычитываем все доступные события, чтобы не уходить в ожидание по каждому.
                    while (_channel.Reader.TryRead(out var sensorEvent))
                    {
                        // Валидация с использованием FrozenDictionary (очень быстро)
                        if (!_eventTypes.ContainsKey(sensorEvent.EventTypeId))
                        {
                            _logger.LogWarning("Received unknown event type {TypeId} from sensor {SensorId}", sensorEvent.EventTypeId, sensorEvent.SensorId);
                            continue;
                        }

                        batch.Add(sensorEvent);

                        // Если батч набрался, отправляем его.
                        if (batch.Count >= 100)
                        {
                            await FlushBatchAsync(batch).ConfigureAwait(false);
                            batch.Clear();
                            // После отправки полного батча также сбрасываем таймер.
                            flushTimerCts.CancelAfter(TimeSpan.FromMilliseconds(500));
                            break; // Выходим из внутреннего while, чтобы снова проверить WaitToReadAsync
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Таймер сработал! Если в батче что-то есть, отправляем.
                    if (batch.Count > 0)
                    {
                        await FlushBatchAsync(batch).ConfigureAwait(false);
                        batch.Clear();
                    }
                    // Пересоздаем CTS, так как он находится вCanceled состоянии и не может быть переиспользован.
                    flushTimerCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));
                }
            }
        }

        private async Task FlushBatchAsync(IReadOnlyCollection<SensorEvent> batch)
        {
            _logger.LogInformation("Flushing batch of {Count} events to storage.", batch.Count);
            // Имитация асинхронной записи в базу данных
            await Task.Delay(Random.Shared.Next(50, 150));
            _logger.LogDebug("Batch flushed successfully.");
        }

        public async Task StopAsync()
        {
            _shutdownCts.Cancel();
            await _processingTask;
        }
    }
}
