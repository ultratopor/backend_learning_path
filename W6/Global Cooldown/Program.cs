using Global_Cooldown;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();

builder.Services.AddTransient<RateLimitMiddleware>();

var app = builder.Build();

app.UseMiddleware<RateLimitMiddleware>();

app.MapGet("/", () => Results.Text("Attack successful! Server took damage.", "text/plain"));

app.Run();