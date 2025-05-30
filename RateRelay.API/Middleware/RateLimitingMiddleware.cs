using System.Collections.Concurrent;
using System.Net;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Options;
using RateRelay.API.Attributes.RateLimiting;
using RateRelay.Domain.Common;
using RateRelay.Infrastructure.Configuration;

namespace RateRelay.API.Middleware;

public class RateLimitingMiddleware(
    RequestDelegate next,
    ILogger<RateLimitingMiddleware> logger,
    IOptions<RateLimitOptions> options)
{
    private readonly RateLimitOptions _options = options.Value;
    private static readonly ConcurrentDictionary<string, RateLimitEntry> RateLimitCache = new();

    public async Task InvokeAsync(HttpContext context)
    {
        if (!_options.EnableRateLimiting)
        {
            await next(context);
            return;
        }

        // dont limit non-api requests
        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            await next(context);
            return;
        }

        var clientIp = GetClientIpAddress(context);
        var endpoint = context.GetEndpoint();

        var globalKey = $"global:{clientIp}";
        if (!await CheckRateLimit(context, globalKey, _options.GlobalLimit, _options.GlobalPeriod))
        {
            return;
        }

        if (endpoint is not null)
        {
            var rateLimitAttribute = GetRateLimitAttribute(endpoint);

            if (rateLimitAttribute is not null)
            {
                var limit = rateLimitAttribute.Limit > 0 ? rateLimitAttribute.Limit : _options.DefaultLimit;
                var period = rateLimitAttribute.PeriodInSeconds > 0
                    ? TimeSpan.FromSeconds(rateLimitAttribute.PeriodInSeconds)
                    : _options.DefaultPeriod;

                var endpointKey = $"{clientIp}:{context.Request.Method}:{context.Request.Path}";

                if (!await CheckRateLimit(context, endpointKey, limit, period))
                {
                    return;
                }
            }
        }

        await next(context);
    }

    private async Task<bool> CheckRateLimit(HttpContext context, string key, int limit, TimeSpan period)
    {
        var entry = RateLimitCache.GetOrAdd(key, _ => new RateLimitEntry
        {
            Count = 0,
            ResetAt = DateTime.UtcNow.Add(period)
        });

        if (DateTime.UtcNow > entry.ResetAt)
        {
            entry = new RateLimitEntry
            {
                Count = 0,
                ResetAt = DateTime.UtcNow.Add(period)
            };
            RateLimitCache[key] = entry;
        }

        Interlocked.Increment(ref entry.Count);

        var remaining = Math.Max(0, limit - entry.Count);
        var resetSeconds = Math.Max(0, (int)(entry.ResetAt - DateTime.UtcNow).TotalSeconds);

        context.Response.Headers[_options.ResponseHeaderLimit] = limit.ToString();
        context.Response.Headers[_options.ResponseHeaderRemaining] = remaining.ToString();
        context.Response.Headers[_options.ResponseHeaderReset] = resetSeconds.ToString();

        if (entry.Count <= limit) return true;
        logger.LogWarning("Rate limit exceeded for {Key}", key);
        context.Response.Headers[_options.ResponseHeaderRetryAfter] = resetSeconds.ToString();

        context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
        context.Response.ContentType = "application/json";

        var response = ApiResponse<object>.Create(
            false,
            errorMessage: "Rate limit exceeded. Please try again later.",
            errorCode: "RATE_LIMIT_EXCEEDED",
            statusCode: StatusCodes.Status429TooManyRequests);

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        return false;
    }

    private static RateLimitAttribute? GetRateLimitAttribute(Endpoint endpoint)
    {
        if (endpoint.Metadata.GetMetadata<ControllerActionDescriptor>() is not { } actionDescriptor) return null;
        var methodAttribute = actionDescriptor.MethodInfo
            .GetCustomAttribute<RateLimitAttribute>(inherit: true);

        if (methodAttribute != null)
        {
            return methodAttribute;
        }

        var controllerAttribute = actionDescriptor.ControllerTypeInfo
            .GetCustomAttribute<RateLimitAttribute>(inherit: true);

        return controllerAttribute;
    }

    private static string GetClientIpAddress(HttpContext context)
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
        {
            ipAddress = forwardedFor.ToString().Split(',')[0].Trim();
        }

        return ipAddress;
    }

    private class RateLimitEntry
    {
        public int Count;
        public DateTime ResetAt;
    }
}

public static class RateLimitingMiddlewareExtensions
{
    public static void UseRateLimiting(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<RateLimitingMiddleware>();
    }
}