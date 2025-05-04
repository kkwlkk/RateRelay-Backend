using Serilog.Context;

namespace RateRelay.API.Middleware;

public class IpLoggingMiddleware(RequestDelegate next, ILogger<IpLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var clientIp = GetClientIp(context);

        using (LogContext.PushProperty("ClientIP", clientIp))
        {
            await next(context);
        }
    }
    
    private static string GetClientIp(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
        {
            return forwardedFor.ToString();
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }
}

public static class IpLoggingMiddlewareExtensions
{
    public static void UseIpLogging(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<IpLoggingMiddleware>();
    }
}