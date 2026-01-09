using Weather_Station;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddKeyedSingleton<IWeatherProvider, LocalSensor>("local");
builder.Services.AddKeyedSingleton<IWeatherProvider, RemoteSatellite>("remote");

builder.Services.AddTransient<SecurityMiddleware>();

var app = builder.Build();

app.UseMiddleware<SecurityMiddleware>();


app.MapGet("/weather/{source}", (string source, IServiceProvider sp) =>
{
    try
    {
        var provider = sp.GetRequiredKeyedService<IWeatherProvider>(source);
        return provider.GetTemperature();
        
    }
    catch 
    {
        return "Unknown source. Try 'local' or 'remote'.";
    }
});

app.Run();
