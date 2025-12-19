using System;
using System.Collections.Frozen;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Event_Gateway;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
var logger = loggerFactory.CreateLogger<Program>();

// --- 1. Конфигурация и FrozenDictionary ---
var eventTypes = new Dictionary<int, string>
            {
                { 1, "Temperature" },
                { 2, "Pressure" },
                { 3, "Humidity" }
            };
// "Замораживаем" словарь для сверхбыстрого чтения
var frozenEventTypes = eventTypes.ToFrozenDictionary();

// --- 2. Создание и запуск основного компонента конвейера ---
var storageBuffer = new StorageBuffer(frozenEventTypes, loggerFactory.CreateLogger<StorageBuffer>());
var channelWriter = storageBuffer.Writer;

// --- 3. Симуляция работы ---
// Запускаем задачу, которая будет имитировать прием данных от датчиков
var simulationTask = Task.Run(async () =>
{
    var random = new Random();
    var sensorIds = Enumerable.Range(0, 10).Select(_ => Guid.NewGuid()).ToArray();

    for (int i = 0; i < 550; i++) // Отправим 550 событий, чтобы проверить и полный батч, и таймаут
    {
        var sensorId = sensorIds[random.Next(sensorIds.Length)];
        var eventTypeId = random.Next(1, 4); // Типы от 1 до 3
        var value = random.NextDouble() * 100;

        var ev = new SensorEvent(sensorId, eventTypeId, value);

        // Отправляем в канал. В реальном приложении это будет делать сетевой обработчик.
        await channelWriter.WriteAsync(ev);

        // Имитируем неравномерный поток событий
        if (i % 150 == 0)
        {
            await Task.Delay(600); // Эта задержка вызовет сброс батча по таймауту
        }
        else
        {
            await Task.Delay(random.Next(1, 20));
        }
    }

    // Завершаем запись в канал, чтобы воркер корректно завершил работу.
    channelWriter.Complete();
});

logger.LogInformation("Simulation started. Waiting for processing to complete...");

// Ждем, пока все события будут обработаны
await simulationTask;
await storageBuffer.StopAsync();

logger.LogInformation("Processing finished.");
