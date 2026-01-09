using Microsoft.Extensions.Caching.Memory;

namespace Global_Cooldown;

public class RateLimitMiddleware(IMemoryCache cache) : IMiddleware
{
    private readonly IMemoryCache _cache = cache;

    private const int Limit = 5;
    private const int WindowSeconds = 10;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown_ip";

        var cacheKey = $"rate_limit_{ipAddress}";

        if (_cache.TryGetValue(cacheKey, out int counter))
        {
            if (counter >= Limit)
            {
                context.Response.StatusCode = 429;
                await context.Response.WriteAsync($"Retry-After {WindowSeconds}");
                return;
            }
            counter++;
            _cache.Set(cacheKey, counter, TimeSpan.FromSeconds(WindowSeconds));
        }
        else
        {
            const int firstRequestCounter = 1;
            _cache.Set(cacheKey, firstRequestCounter, TimeSpan.FromSeconds(WindowSeconds));
        }

        await next(context);
    }
}