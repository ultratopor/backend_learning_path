using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ConfigWatcher;

internal class Program
{
    
    static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddSingleton<FileWatcherService>(sp=>
        {
            var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");

            if (!File.Exists(path)) File.WriteAllText(path, "{\"test\": 1}");

            return new FileWatcherService(path);
        });
        builder.Services.AddHostedService<FileWatcherService>(sp => sp.GetRequiredService<FileWatcherService>());
        builder.Services.AddHostedService<GreetingService>();
        var host = builder.Build();

        host.Run();
    }
}
