using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Rotation_Logger;

internal class Program
{
   
    static async Task Main(string[] args)
    {
        long maxFileSizeInBytes = 10 * 1024; // 10 KB
        string logPath = Path.Combine(AppContext.BaseDirectory, "logs", "app.log");

        var builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddSingleton<RotationLogger>(sp =>         {
            return new RotationLogger(logPath, maxFileSizeInBytes, bufferCapacity: 1000);
        });

        builder.Services.AddHostedService<BackgroundFileService>(sp => sp.GetRequiredService<BackgroundFileService>());

        var host = builder.Build();
        await host.RunAsync();

        
    }
}
