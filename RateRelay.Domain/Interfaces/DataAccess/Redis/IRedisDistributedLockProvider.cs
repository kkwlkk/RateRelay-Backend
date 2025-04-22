using RateRelay.Domain.Enums.Redis;

namespace RateRelay.Domain.Interfaces.DataAccess.Redis;

public interface IRedisDistributedLockProvider
{
    Task<IAsyncDisposable?> TryAcquireLockAsync(DistributedLockCategory category, string key, TimeSpan? duration = null);
    bool IsLockAcquired(DistributedLockCategory category, string key);
    Task<bool> IsLockAcquiredAsync(DistributedLockCategory category, string key);
    Task<bool> ForceReleaseLockAsync(DistributedLockCategory category, string key);
}