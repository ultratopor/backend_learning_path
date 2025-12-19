using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Channels;

namespace OrderProcessingServer
{
    public class ProductionOrderStatusManager
    {
        // --- 1. Управление ресурсами и Backpressure ---
        private readonly PaddedOrderStatus[] _statusArray;
        private readonly ConcurrentDictionary<Guid, int> _orderToIndex = new();
        private readonly ConcurrentStack<int> _freeIndices;
        private readonly SemaphoreSlim _concurrencyLimiter; 

        public ProductionOrderStatusManager(int maxConcurrentOrders)
        {
            _statusArray = new PaddedOrderStatus[maxConcurrentOrders];
            _freeIndices = new ConcurrentStack<int>();
            _concurrencyLimiter = new SemaphoreSlim(maxConcurrentOrders, maxConcurrentOrders);

            for (int i = maxConcurrentOrders - 1; i >= 0; i--)
            {
                _freeIndices.Push(i);
            }
        }

        // --- 2. Создание заказа (теперь с ожиданием) ---
        public async Task<Guid> CreateOrderAsync()
        {
            await _concurrencyLimiter.WaitAsync();

            var orderId = Guid.NewGuid();
            var newStatus = new PaddedOrderStatus(DateTime.UtcNow, OrderStatusCode.Received); // 1 = Received

            // --- 3. Атомарное добавление (решение Race Condition) ---
            int index = _orderToIndex.GetOrAdd(orderId, _ =>
            {
                if (!_freeIndices.TryPop(out var newIndex))
                {
                    throw new InvalidOperationException("Семафор и пул индексов рассинхронизированы.");
                }
                _statusArray[newIndex] = newStatus;
                return newIndex;
            });

            return orderId;
        }

        // --- 4. Обновление статуса (теперь безопасно для новых и старых заказов) ---
        public bool UpdateStatus(Guid orderId, OrderStatusCode statusCode)
        {
            if (_orderToIndex.TryGetValue(orderId, out int index))
            {
                _statusArray[index] = new PaddedOrderStatus(DateTime.UtcNow, statusCode);
                return true;
            }
            return false; 
        }

        // --- 5. Завершение заказа (решение Утечки Индексов) ---
        public void CompleteOrder(Guid orderId)
        {
            if (_orderToIndex.TryRemove(orderId, out int index))
            {
                _freeIndices.Push(index);
            }

            _concurrencyLimiter.Release();
        }

        public bool TryGetStatus(Guid orderId, out PaddedOrderStatus status)
        {
            if (_orderToIndex.TryGetValue(orderId, out int index))
            {
                status = _statusArray[index];
                return true;
            }
            status = default;
            return false;
        }

        public void Dispose()
        {
            _concurrencyLimiter?.Dispose();
        }
    }
    internal class OrderProcessingService(int maxConcurrentOrders)
    {
        private const int CannelCapacity = 1000;
        private readonly Channel<Order> _channel = Channel.CreateBounded<Order>(CannelCapacity);
        private readonly ProductionOrderStatusManager _statusManager = new(maxConcurrentOrders);

        public async Task RunProducerAsync(int orderCount)
        {
            for (int i = 0; i < orderCount; i++)
            {
                var orderId = await _statusManager.CreateOrderAsync();
                var order = new Order(orderId, $"Product {i}", i * 10.0m);
                await _channel.Writer.WriteAsync(order);
                UpdateStatus(order.Id, OrderStatusCode.Received);
                Console.WriteLine($"[Producer] Заказ {order.Id} отправлен в канал");
            }

            _channel.Writer.Complete();
            Console.WriteLine("[Producer] Все заказы отправлены. Канал закрыт для записи.");
        }

        public async Task RunConsumerAsync(int workerId, CancellationToken cancellationToken)
        {
            Console.WriteLine($"[Consumer {workerId}] запущен.");

            await foreach (var order in _channel.Reader.ReadAllAsync(cancellationToken))
            {
                Console.WriteLine($"[Consumer {workerId}] начал обработку заказа {order.Id}.");
                UpdateStatus(order.Id, OrderStatusCode.Processing);
                await Task.Delay(Random.Shared.Next(500, 2000), cancellationToken);
                UpdateStatus(order.Id, OrderStatusCode.Completed);
                _statusManager.CompleteOrder(order.Id);
                Console.WriteLine($"[Consumer {workerId}] завершил обработку заказа {order.Id}.");
            }

            Console.WriteLine($"[Consumer {workerId}] канал пуст и закрыт. Воркер останавливается.");
            _statusManager.Dispose();
        }

        public bool TryGetStatus(Guid orderId, out PaddedOrderStatus status)
        {
            if (_statusManager.TryGetStatus(orderId, out status))
                return true;

            status = default;
            return false;
        }

        public void UpdateStatus(Guid orderId, OrderStatusCode statusCode)
        {
            _statusManager.UpdateStatus(orderId, statusCode);
        }
    }


    public record Order(Guid Id, string ProductName, decimal Price);
    

    [StructLayout(LayoutKind.Explicit)]
    public struct PaddedOrderStatus
    {
        [FieldOffset(0)]
        public DateTime Timestamp;

        [FieldOffset(8)]
        public OrderStatusCode StatusCode;

        [FieldOffset(12)]
        private readonly long _padding1;

        [FieldOffset(20)]
        private readonly long _padding2;

        [FieldOffset(28)]
        private readonly long _padding3;

        [FieldOffset(36)]
        private readonly long _padding4;

        [FieldOffset(44)]
        private readonly long _padding5;

        [FieldOffset(52)]
        private readonly long _padding6;

        [FieldOffset(60)]
        private readonly int _padding7;

        public PaddedOrderStatus(DateTime timestamp, OrderStatusCode statusCode)
        {
            this.Timestamp = timestamp;
            this.StatusCode = statusCode;
        }
    }

    public enum OrderStatusCode
    {
        Received = 1,
        Processing = 2,
        Completed = 3
    }
}
