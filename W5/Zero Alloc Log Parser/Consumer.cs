using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;

namespace Zero_Alloc_Log_Parser;

internal static class Consumer
{
    private static ReadOnlySpan<byte> ErrorBytes => "ERROR"u8;
    private static int count = 0;
    public static async Task ProcessLogAsync(PipeReader reader)
    {
        while (true)
        {
            ReadResult result = await reader.ReadAsync();
            ReadOnlySequence<byte> buffer = result.Buffer;

            while (TryReadLogLine(ref buffer, out ReadOnlySequence<byte> line))
            {
                ProcessLine(line); // Поиск "ERROR" внутри line
            }

            // К этому моменту buffer содержит только неполный остаток данных.
            // Мы говорим пайпу:
            // 1. Consumed: buffer.Start (всё до текущего начала потреблено)
            // 2. Examined: buffer.End (мы посмотрели всё до конца и не нашли \n)
            reader.AdvanceTo(buffer.Start, buffer.End);

            if (result.IsCompleted) break;
        }
        await reader.CompleteAsync();
    }

    private static bool TryReadLogLine(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> line)
    {
        // Ищем \n. Если нашли - возвращаем true и модифицируем buffer (срез)
        SequencePosition? position = buffer.PositionOf((byte)'\n');
        if (position == null)
        {
            line = default;
            return false;
        }

        // Включаем \n в строку или исключаем - зависит от логики, 
        // здесь сдвигаемся СРАЗУ ПОСЛЕ \n
        var nextLineStart = buffer.GetPosition(1, position.Value);
        line = buffer.Slice(0, position.Value);
        buffer = buffer.Slice(nextLineStart);
        return true;
    }

    private static void ProcessLine(ReadOnlySequence<byte> line)
    {
        var reader = new SequenceReader<byte>(line);

        if (line.IsSingleSegment)
        {

            if (line.First.Span.IndexOf(ErrorBytes) >= 0)
            {
                count++;
            }
        }
        else
        {
            if (ContainSequence(ref reader, ErrorBytes))
                count++;
        }
    }

    private static bool ContainSequence(ref SequenceReader<byte> reader, ReadOnlySpan<byte> value)
    {
        return reader.TryReadTo(out ReadOnlySpan<byte> _, value, advancePastDelimiter: false);
    }
}
