using BenchmarkDotNet.Attributes;
using System;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

[MemoryDiagnoser]
public class StringAllocations
{
    private const int IterationCount = 1000;

    [Benchmark(Baseline = true)]
    public string Naive_Plus()
    {
        string id = "TX-";
        id += 12345.ToString();
        id += "-END";
        return id;
    }

    [Benchmark]
    public string StringBuilder_NoCapacity()
    {
        var sb = new StringBuilder();
        sb.Append("TX-");
        sb.Append(12345);
        sb.Append("-END");
        return sb.ToString();
    }

    [Benchmark]
    public string StringBuilder_WithCapacity()
    {
        // 3 chars + 5 digits + 4 chars = 12
        var sb = new StringBuilder(12);
        sb.Append("TX-");
        sb.Append(12345);
        sb.Append("-END");
        return sb.ToString();
    }

    [Benchmark]
    public string String_Create_ZeroAlloc()
    {
        // ВАША ЗАДАЧА: Реализовать генерацию "TX-12345-END"
        // через string.Create.
        // Используйте writer.Write или TryFormat.
        // Не используйте .ToString() внутри!

        return string.Create(12, 12345, (span, number) =>
        {
            int currentOffset = 0;
            int charsWritten;

            ReadOnlySpan<char> prefix = "TX-".AsSpan();
            prefix.CopyTo(span.Slice(currentOffset, prefix.Length));
            currentOffset += prefix.Length;
            
            number.TryFormat(span.Slice(currentOffset, 5), out charsWritten);
            currentOffset += charsWritten;
            /*
            var tempSpan = span.Slice(currentOffset, number.ToString().Length);
            tempSpan.Fill('0');
            for(int i = tempSpan.Length -1; i >= 0; i--)
            {
                if(number == 0) break;
                tempSpan[i] = (char)('0' + (number % 10));
                number /= 10;
            }
            */
            ReadOnlySpan<char> suffix = "-END".AsSpan();
            suffix.CopyTo(span.Slice(currentOffset, suffix.Length));
            currentOffset += suffix.Length;

            if(currentOffset != span.Length)
            {
                throw new InvalidOperationException("Did not write the expected number of characters.");
            }
        });
    }

    private const string Prefix = "TX-";
    private const string Suffix = "-END";
    private const int Number = 12345;
    private int FinalStringLength = Prefix.Length + Number.ToString().Length + Suffix.Length; // 3 + 5 + 4 = 12
    private const int NumberStartIndex = 3;
    private const int NumberLength = 5;

    [Benchmark]
    public string String_Create_Manual()
    {
        return string.Create(FinalStringLength, Number, (span, num) =>
        {
            Prefix.AsSpan().CopyTo(span);
            Suffix.AsSpan().CopyTo(span.Slice(NumberStartIndex + NumberLength));

            var numberSpan = span.Slice(NumberStartIndex, NumberLength);

            numberSpan.Fill('0');

            if (num == 0) return;

            int writeIndex = NumberLength - 1; // Начинаем запись с конца (справа налево)
            int currentNum = num;

            while (currentNum > 0)
            {
                int digit = currentNum % 10;

                char charToWrite = (char)('0' + digit);

                numberSpan[writeIndex] = charToWrite;

                writeIndex--;

                currentNum /= 10;
            }
        });
    }
}
