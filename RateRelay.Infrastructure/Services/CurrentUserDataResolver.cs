using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Enums;
using RateRelay.Domain.Interfaces;
using RateRelay.Domain.Interfaces.DataAccess;

namespace RateRelay.Infrastructure.Services;

public class CurrentUserDataResolver(
    IHttpContextAccessor httpContextAccessor,
    IUnitOfWorkFactory unitOfWorkFactory,
    IMemoryCache memoryCache)
    : ICurrentUserDataResolver
{
    private const string UserDataCacheKeyPrefix = "UserData_";
    private const int CacheExpirationMinutes = 1;

    public long GetAccountId()
    {
        if (TryGetAccountId(out var accountId))
        {
            return accountId;
        }

        throw new InvalidOperationException("Account ID not found in claims or user is not authenticated.");
    }

    public async Task<AccountFlags> GetAccountFlagsAsync()
    {
        if (!TryGetAccountId(out var accountId))
        {
            return AccountFlags.None;
        }

        var userData = await GetCachedUserDataAsync(accountId);
        return userData.AccountFlags;
    }

    public AccountFlags GetAccountFlags()
    {
        return GetAccountFlagsAsync().GetAwaiter().GetResult();
    }

    public async Task<string> GetUsernameAsync()
    {
        if (!TryGetAccountId(out var accountId))
        {
            throw new InvalidOperationException("User is not authenticated.");
        }

        var userData = await GetCachedUserDataAsync(accountId);
        return userData.Username;
    }

    public string GetUsername()
    {
        return GetUsernameAsync().GetAwaiter().GetResult();
    }

    public async Task<ulong> GetPermissionsAsync()
    {
        if (!TryGetAccountId(out var accountId))
        {
            return 0;
        }

        var userData = await GetCachedUserDataAsync(accountId);
        return userData.Permissions;
    }

    public ulong GetPermissions()
    {
        return GetPermissionsAsync().GetAwaiter().GetResult();
    }

    private async Task<CachedUserData> GetCachedUserDataAsync(long accountId)
    {
        var cacheKey = $"{UserDataCacheKeyPrefix}{accountId}";

        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var userRepository = unitOfWork.GetRepository<AccountEntity>();

        if (memoryCache.TryGetValue(cacheKey, out CachedUserData cachedData))
        {
            var user = await userRepository.GetByIdAsync(accountId);
            if (user != null && user.DateModifiedUtc > DateTime.UtcNow.AddMinutes(CacheExpirationMinutes))
            {
                return cachedData;
            }

            memoryCache.Remove(cacheKey);
        }

        var freshUser = await userRepository.GetByIdAsync(accountId);
        if (freshUser == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        var userData = new CachedUserData
        {
            Username = freshUser.GoogleUsername,
            Email = freshUser.Email,
            Permissions = freshUser.Permissions,
            AccountFlags = freshUser.Flags,
            CachedAt = DateTime.UtcNow
        };

        memoryCache.Set(cacheKey, userData, TimeSpan.FromMinutes(CacheExpirationMinutes));
        return userData;
    }

    public void InvalidateUserCache(long accountId)
    {
        var cacheKey = $"{UserDataCacheKeyPrefix}{accountId}";
        memoryCache.Remove(cacheKey);
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

    public string GetEmail()
    {
        if (!TryGetAccountId(out var accountId))
        {
            return string.Empty;
        }

        var userData = GetCachedUserDataAsync(accountId).GetAwaiter().GetResult();
        return userData.Email;
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

    private class CachedUserData
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public ulong Permissions { get; set; }
        public AccountFlags AccountFlags { get; set; }
        public DateTime CachedAt { get; set; }
    }
}