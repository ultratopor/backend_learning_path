using System.Text;
using System.Threading.Channels;

namespace Rotation_Logger;

internal readonly record struct LogEntry(string Message)
{
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

internal class RotationLogger : IDisposable
{
    private readonly    Channel<LogEntry>   _logChannel;
    private readonly    string              _path;
    private readonly    long                _maxFileSizeInBytes;
    private const       int                _maxBackups = 5;

    public RotationLogger(string path, long maxFileSizeInBytes, int bufferCapacity)
    {
        _path = path;
        _maxFileSizeInBytes = maxFileSizeInBytes;

        var options = new BoundedChannelOptions(bufferCapacity)
        {
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false,
            FullMode = BoundedChannelFullMode.DropOldest
        };

        _logChannel = Channel.CreateBounded<LogEntry>(options);
    }

    public async ValueTask LogAsync(string message)
    {
        await _logChannel.Writer.WriteAsync(new LogEntry(message));
    }

    public async Task ProcessQueue(CancellationToken ct)
    {
        var stream = OpenFile();

        try
        {
            while (await _logChannel.Reader.WaitToReadAsync(ct))
            {
                while (_logChannel.Reader.TryRead(out var logEntry))
                {
                    if (stream.Length > _maxFileSizeInBytes) stream = await RotateLogAsync(stream);
                    await stream.WriteAsync(Encoding.UTF8.GetBytes($"{logEntry.Timestamp:O} - {logEntry.Message}{Environment.NewLine}"), ct);
                }

                await stream.FlushAsync(ct);
            }
        }
        finally
        {
            await stream.DisposeAsync();
        }
    }

    private FileStream OpenFile()
    {
        var directory = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory); 
        }

        return new FileStream(_path, FileMode.Append, FileAccess.Write, FileShare.Read);
    }

    private async Task<FileStream> RotateLogAsync(FileStream currentStream)
    {
        await currentStream.FlushAsync();
        await currentStream.DisposeAsync();

        try
        {
            for(int i = _maxBackups-1; i >= 1; i--)
            {
                string source = $"{_path}.{i}";
                string dest = $"{_path}.{i + 1}";

                if (File.Exists(source)) File.Move(source, dest, overwrite: true);
            }

            string firstBackup = $"{_path}.1";
            File.Move(_path, firstBackup, overwrite: true);
        }
        catch(IOException ex)
        {
            Console.WriteLine($"Rotation failed: {ex.Message}");
        }

        return OpenFile();
    }

    public void Dispose()
    {
        _logChannel.Writer.TryComplete();
    }
}
