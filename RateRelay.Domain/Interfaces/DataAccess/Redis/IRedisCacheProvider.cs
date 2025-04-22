using RateRelay.Domain.Enums.Redis;

namespace RateRelay.Domain.Interfaces.DataAccess.Redis;

public interface IRedisCacheProvider
{
    Task SetAsync<T>(CacheEntryCategory category, string key, T value, TimeSpan? expiry = null);
    Task<T?> GetAsync<T>(CacheEntryCategory category, string key);
    Task<bool> RemoveAsync(CacheEntryCategory category, string key);
    Task<bool> KeyExistsAsync(CacheEntryCategory category, string key);
}