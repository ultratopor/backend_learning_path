using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;

[MemoryDiagnoser]
public class ParsingBenchmarks
{
    private const string InputString = "User:John;Age:30;Role:Admin";

    [Benchmark(Baseline = true)]
    public Dictionary<string, string> Parse_Legacy()
    {
        var result = new Dictionary<string, string>();
        var parts = InputString.Split(';');
        foreach (var part in parts)
        {
            var keyValue = part.Split(':');
            result[keyValue[0]] = keyValue[1];
        }
        return result;
    }

    [Benchmark]
    public int Parse_ModernZeroAlloc()
    {
        // Цель: найти значение "Age" и вернуть его как int, не создавая промежуточных строк.
        ReadOnlySpan<char> fullSpan = InputString.AsSpan();

        // 1. Находим начало "Age:"
        int ageKeyIndex = fullSpan.IndexOf("Age:");
        if (ageKeyIndex == -1) return -1; // Не нашли

        // 2. Делаем срез, начиная со значения "30"
        var ageValueSpan = fullSpan.Slice(ageKeyIndex + "Age:".Length);

        // 3. Находим конец значения (символ ';')
        int semicolonIndex = ageValueSpan.IndexOf(';');
        if (semicolonIndex == -1) return -1; // Не нашли

        // 4. Делаем финальный срез, содержащий только "30"
        var finalNumberSpan = ageValueSpan.Slice(0, semicolonIndex);

        // 5. Парсим число напрямую из спана. Эта операция не аллоцирует память.
        return int.Parse(finalNumberSpan);
    }
}
