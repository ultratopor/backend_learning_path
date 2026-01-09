namespace Weather_Station;

public class SecurityMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // Проверяем наличие токена в query параметрах
        if (!context.Request.Query.TryGetValue("token", out var tokenValue) || tokenValue != "admin")
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        await next(context);
    }
}