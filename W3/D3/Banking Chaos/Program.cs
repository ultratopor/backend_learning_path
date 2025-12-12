using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

public class BankAccount
{
    public int Balance;
    private readonly object _lock = new object(); // Для блокировок в Части 3

    // --- Часть 1: Метод с Race Condition ---
    public void Deposit_WithRaceCondition(int amount)
    {
        Balance += amount;
    }

    // --- Часть 2: Метод, исправленный с помощью Interlocked ---
    public void Deposit_WithInterlocked(int amount)
    {
        Interlocked.Add(ref Balance, amount);
    }

    // --- Часть 3: Метод, приводящий к Deadlock ---
    public void Transfer(BankAccount to, int amount)
    {
        lock (this)
        {
            Console.WriteLine($"Поток {Thread.CurrentThread.ManagedThreadId}: Заблокировал счет {this.GetHashCode()}");
            Thread.Sleep(10);
            Console.WriteLine($"Поток {Thread.CurrentThread.ManagedThreadId}: Пытается заблокировать счет {to.GetHashCode()}");
            lock (to)
            {
                this.Balance -= amount;
                to.Balance += amount;
            }
        }
    }
}

public class Program
{
    public static void Main()
    {
        Console.WriteLine("--- Часть 1: Демонстрация Race Condition ---");
        RaceConditionDemo();
        Console.WriteLine("\n--- Часть 2: Исправление с помощью Interlocked ---");
        InterlockedDemo();
        Console.WriteLine("\n--- Часть 3: Демонстрация Deadlock (программа зависнет) ---");
        DeadlockDemo();
    }

    private static void RaceConditionDemo()
    {
        const int taskCount = 10;
        const int depositsPerTask = 1000;
        var account = new BankAccount();

        var tasks = new List<Task>();
        for (int i = 0; i < taskCount; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < depositsPerTask; j++)
                {
                    account.Deposit_WithRaceCondition(1);
                }
            }));
        }

        Task.WaitAll(tasks.ToArray());
        Console.WriteLine($"Ожидаемый баланс: {taskCount * depositsPerTask}");
        Console.WriteLine($"Реальный баланс:   {account.Balance}");
        Console.WriteLine("Потеряно денег из-за гонки!");
    }

    private static void InterlockedDemo()
    {
        const int taskCount = 10;
        const int depositsPerTask = 1000;
        var account = new BankAccount();

        var tasks = new List<Task>();
        for (int i = 0; i < taskCount; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < depositsPerTask; j++)
                {
                    account.Deposit_WithInterlocked(1);
                }
            }));
        }

        Task.WaitAll(tasks.ToArray());
        Console.WriteLine($"Ожидаемый баланс: {taskCount * depositsPerTask}");
        Console.WriteLine($"Реальный баланс:   {account.Balance}");
        Console.WriteLine("Баланс сходится! Interlocked спас ситуацию.");
    }

    private static void DeadlockDemo()
    {
        var accountA = new BankAccount { Balance = 1000 };
        var accountB = new BankAccount { Balance = 1000 };

        Console.WriteLine($"Начальный баланс A: {accountA.Balance}, B: {accountB.Balance}");

        // Поток 1 пытается перевести из А в Б
        var task1 = Task.Run(() =>
        {
            Console.WriteLine("Поток 1: Начинает перевод A -> B");
            accountA.Transfer(accountB, 1);
            Console.WriteLine("Поток 1: Завершил перевод A -> B");
        });

        // Поток 2 пытается перевести из Б в А
        var task2 = Task.Run(() =>
        {
            Console.WriteLine("Поток 2: Начинает перевод B -> A");
            accountB.Transfer(accountA, 1);
            Console.WriteLine("Поток 2: Завершил перевод B -> A");
        });

        try
        {
            // Устанавливаем таймаут, чтобы программа не висела вечно
            Task.WaitAll(new[] { task1, task2 }, TimeSpan.FromSeconds(50));
        }
        catch (AggregateException ex)
        {
            Console.WriteLine("Что-то пошло не так (или это таймаут): " + ex.Message);
        }

        Console.WriteLine($"Итоговый баланс A: {accountA.Balance}, B: {accountB.Balance}");
        Console.WriteLine("Если программа дошла до сюда, deadlock'а не произошло. Если зависла - это он.");
    }
}
