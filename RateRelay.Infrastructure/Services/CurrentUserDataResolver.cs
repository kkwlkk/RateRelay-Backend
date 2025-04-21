using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using RateRelay.Domain.Interfaces;

namespace RateRelay.Infrastructure.Services;

public class CurrentUserDataResolver(
    IHttpContextAccessor httpContextAccessor,
    IMemoryCache memoryCache)
    : ICurrentUserDataResolver
{
    private readonly IMemoryCache _memoryCache = memoryCache;
    private const string AccountCacheKeyPrefix = "UserAccount_";
    private const int CacheExpirationMinutes = 5;

    public long GetAccountId()
    {
        if (TryGetAccountId(out var accountId))
        {
            return accountId;
        }

        throw new InvalidOperationException("Account ID not found in claims or user is not authenticated.");
    }

    public bool TryGetAccountId(out long accountId)
    {
        accountId = 0;

        if (httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        var subClaim = httpContextAccessor.HttpContext.User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(subClaim) || !long.TryParse(subClaim, out accountId))
        {
            return false;
        }

        return true;
    }

    public string GetUsername()
    {
        var username = GetClaimValue<string>("name");

        if (string.IsNullOrEmpty(username))
        {
            throw new InvalidOperationException("Username not found in claims or user is not authenticated.");
        }

        return username;
    }

    public string GetEmail()
    {
        return GetClaimValue<string>("email", string.Empty);
    }

    public ulong GetPermissions()
    {
        var permissionsString = GetClaimValue<string>("permissions");

        if (string.IsNullOrEmpty(permissionsString) || !ulong.TryParse(permissionsString, out var permissions))
        {
            return 0;
        }

        return permissions;
    }

    public bool IsAuthenticated()
    {
        return httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;
    }

    public T GetClaimValue<T>(string claimType, T defaultValue = default)
    {
        if (httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return defaultValue;
        }

        var claim = httpContextAccessor.HttpContext.User.FindFirst(claimType);

        if (claim == null)
        {
            return defaultValue;
        }

        try
        {
            if (typeof(T) == typeof(string))
            {
                return (T)(object)claim.Value;
            }

            return (T)Convert.ChangeType(claim.Value, typeof(T));
        }
        catch
        {
            return defaultValue;
        }
    }
}