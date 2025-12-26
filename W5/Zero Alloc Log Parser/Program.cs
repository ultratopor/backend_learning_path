using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text;

namespace Zero_Alloc_Log_Parser;

public class Program
{
    public static async Task Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Usage: Zero_Alloc_Log_Parser <logfile>");
            return;
        }
        string filePath = args[0];
        
        await RunLogParserAsync(filePath);
    }

    private static async Task RunLogParserAsync(string filePath)
    {
        var pipe = new Pipe();

        var producer = Producer.FillPipeAsync(filePath, pipe.Writer);
        var consumer = Consumer.ProcessLogAsync(pipe.Reader);
        await Task.WhenAll(producer, consumer);
    }
}