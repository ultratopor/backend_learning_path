using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;

namespace Zero_Alloc_Log_Parser;

internal static class Producer
{
    public static async Task FillPipeAsync(string filePath, PipeWriter writer)
    {
        const int minimumBufferSize = 512;

        using FileStream fs = new(filePath, 
            new FileStreamOptions
            {
                Mode = FileMode.Open,
                Access = FileAccess.Read,
                Options = FileOptions.Asynchronous | FileOptions.SequentialScan
            });

        while (true)
        {
            Memory<byte> memory = writer.GetMemory(minimumBufferSize);
            int bytesRead = await fs.ReadAsync(memory);
            if (bytesRead == 0) break;
            // Сообщаем пайпу, сколько байт мы записали
            writer.Advance(bytesRead);
            // Даем пайпу знать, что данные готовы для чтения
            FlushResult result = await writer.FlushAsync();
            if (result.IsCompleted) break;
        }
        await writer.CompleteAsync();
    }
}
