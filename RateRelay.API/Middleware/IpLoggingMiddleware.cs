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

        var remoteIp = context.Connection.RemoteIpAddress;

        if (remoteIp is null) return "Unknown";
        return remoteIp.IsIPv4MappedToIPv6 ? remoteIp.MapToIPv4().ToString() : remoteIp.ToString();
    }
}

public static class IpLoggingMiddlewareExtensions
{
    public static void UseIpLogging(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<IpLoggingMiddleware>();
    }
}