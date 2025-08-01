using System.Net;
using Microsoft.AspNetCore.Http;

namespace RateRelay.Infrastructure.Extensions;

public static class HttpRequestExtensions
{
    public static bool IsLocal(this HttpRequest req)
    {
        var connection = req.HttpContext.Connection;
        if (connection.RemoteIpAddress != null)
        {
            return connection.LocalIpAddress != null
                ? connection.RemoteIpAddress.Equals(connection.LocalIpAddress)
                : IPAddress.IsLoopback(connection.RemoteIpAddress);
        }

        return true;
    }
}