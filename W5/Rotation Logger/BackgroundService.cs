using Microsoft.Extensions.Hosting;

namespace Rotation_Logger;

internal class BackgroundFileService(RotationLogger logger) : BackgroundService
{
    private readonly RotationLogger _logger = logger;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return _logger.ProcessQueue(stoppingToken);
    }

    public override void Dispose()
    {
        _logger.Dispose();
    }
}
