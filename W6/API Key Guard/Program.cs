using System.Text;

namespace API_Key_Guard;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddScoped<IKeyValidator, FakeKeyValidator>();
        builder.Services.AddTransient<ApiKeyMiddleware>();


        var app = builder.Build();

        app.UseMiddleware<ApiKeyMiddleware>();

        app.Run();
    }
}

public interface IKeyValidator
{
    bool IsValid(string key);
}

public class FakeKeyValidator : IKeyValidator
{
    private List<string> validKeys = new List<string>
    {
        "VALID_KEY_1",
        "VALID_KEY_2",
        "VALID_KEY_3"
    };

    public bool IsValid(string key)
    {
        return validKeys.Contains(key);
    }
}

public class ApiKeyMiddleware(IKeyValidator keyValidator) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (!context.Request.Headers.TryGetValue("X-API-KEY", out var extractedApiKey))
        {
            context.Response.StatusCode = 401; // Unauthorized
            await context.Response.WriteAsync("API Key is missing");
            return;
        }
        if (!keyValidator.IsValid(extractedApiKey))
        {
            context.Response.StatusCode = 403; // Forbidden
            await context.Response.WriteAsync("Invalid API Key");
            return;
        }
        await next(context);
    }
}

public class SpyMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var originalBodyStream = EnterRequest(context).Result;

        using var responseBodyStream = new MemoryStream();

        context.Response.Body = responseBodyStream;

        await next(context);

        context.Response.Body.Position = 0;

        string responseBodyText;
        using (var reader = new StreamReader(context.Response.Body, Encoding.UTF8, leaveOpen: true))
        {
            responseBodyText = await reader.ReadToEndAsync();
        }

        if (responseBodyText.Contains("Agent"))
        {
            responseBodyText = responseBodyText.Replace("Agent", "REDACTED");

            context.Response.Headers.Remove("Content-Length");
        }

        context.Response.Body = originalBodyStream;

        await context.Response.WriteAsync(responseBodyText);
    }

    private static async Task<Stream> EnterRequest(HttpContext context)
    {
        context.Request.EnableBuffering();

        var requestStream = new MemoryStream();
        await context.Request.Body.CopyToAsync(requestStream);

        requestStream.Position = 0;

        using (var reader = new StreamReader(requestStream, encoding: Encoding.UTF8, leaveOpen: true))
        {
            var body = await reader.ReadToEndAsync();
        }

        requestStream.Position = 0;

        //context.Request.Body = requestStream;

        return requestStream;
    }
}
