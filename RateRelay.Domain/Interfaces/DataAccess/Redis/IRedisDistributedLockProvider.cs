using RateRelay.Domain.Enums.Redis;

namespace RateRelay.Domain.Interfaces.DataAccess.Redis;

public interface IRedisDistributedLockProvider
{
    /// <summary>
    /// Tries to acquire a distributed lock for the specified category and key.
    /// </summary>
    /// <param name="category"></param>
    /// <param name="key"></param>
    /// <param name="duration">Optional duration of how long it will try to acquire the lock.</param>
    /// <returns></returns>
    Task<IAsyncDisposable?> TryAcquireLockAsync(DistributedLockCategory category, string key, TimeSpan? duration = null);
    /// <summary>
    /// Tries to acquire a persistent distributed lock for the specified category and key.
    /// </summary>
    Task<bool> TryAcquirePersistentLockAsync(DistributedLockCategory category, string key, TimeSpan lockDuration);
    bool IsLockAcquired(DistributedLockCategory category, string key);
    Task<bool> IsLockAcquiredAsync(DistributedLockCategory category, string key);
    Task<bool> ForceReleaseLockAsync(DistributedLockCategory category, string key);
    Task<TimeSpan?> GetLockTtlAsync(DistributedLockCategory category, string key);
}