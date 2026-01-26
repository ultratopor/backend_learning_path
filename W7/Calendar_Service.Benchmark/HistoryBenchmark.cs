using BenchmarkDotNet.Attributes;
using Calendar_Service.Data;
using Calendar_Service.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Calendar_Service.Benchmarks;

[MemoryDiagnoser] // <--- Самый важный атрибут! Показывает аллокации памяти (GC)
public class HistoryBenchmark
{
    private ApplicationDbContext _context;
    private Guid _targetUserId;

    [GlobalSetup]
    public void Setup()
    {
        // 1. Настраиваем контекст (как в Program.cs)
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=CalendarDb;Username=postgres;Password=mysecretpassword;Pooling=true;")
            .UseSnakeCaseNamingConvention()
            .Options;

        _context = new ApplicationDbContext(options);

        // 2. Ищем "жертву" для тестов (Илона)
        // Если база пустая - упадет. Убедись, что Илон там есть.
        var user = _context.Users.FirstOrDefault(u => u.Username == "Elon Musk")
                   ?? _context.Users.First();

        _targetUserId = user.Id;
    }

    // ТЕСТ 1: Как делают новички (Tracking + Single Query)
    [Benchmark(Baseline = true)]
    public List<Booking> Naive_SingleQuery_Tracking()
    {
        return _context.Users
            // По умолчанию Tracking включен
            .Include(u => u.Bookings)
            .AsSingleQuery() // Огромный JOIN
            .First(u => u.Id == _targetUserId)
            .Bookings
            .ToList();
    }

    // ТЕСТ 2: Как делают профи (NoTracking + Split Query)
    [Benchmark]
    public List<Booking> Optimized_SplitQuery_NoTracking()
    {
        return _context.Users
            .AsNoTracking() // Не засоряем ChangeTracker
            .Include(u => u.Bookings)
            .AsSplitQuery() // Два легких запроса вместо одного тяжелого
            .First(u => u.Id == _targetUserId)
            .Bookings
            .ToList();
    }
}