using Medallion.Threading.Redis;
using RateRelay.Domain.Enums.Redis;
using RateRelay.Domain.Interfaces.DataAccess.Redis;
using Serilog;
using StackExchange.Redis;

namespace RateRelay.Infrastructure.DataAccess.Redis;

public class RedisDistributedLockProvider(IConnectionMultiplexer connectionMultiplexer, ILogger logger)
    : IRedisDistributedLockProvider
{
    private readonly IConnectionMultiplexer _connectionMultiplexer =
        connectionMultiplexer ?? throw new ArgumentNullException(nameof(connectionMultiplexer));

    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<IAsyncDisposable?> TryAcquireLockAsync(DistributedLockCategory category, string key,
        TimeSpan? duration = null)
    {
        var database = _connectionMultiplexer.GetDatabase();
        var lockKey = GetLockKey(category, key);
        var distributedLock = new RedisDistributedLock(lockKey, database);

        try
        {
            return await distributedLock.TryAcquireAsync(duration ?? TimeSpan.Zero);
        }
        catch (RedisConnectionException ex)
        {
            _logger.Error(ex,
                "Redis connection error occurred while trying to acquire a lock for category: {LockCategory}, name: {LockName}, duration: {LockDuration}",
                category, lockKey, duration);

            throw;
        }
        catch (Exception ex)
        {
            _logger.Error(ex,
                "An error occurred while trying to acquire a lock for category: {LockCategory}, name: {LockName}, duration: {LockDuration}",
                category, lockKey, duration);

            throw;
        }
    }

    public async Task<bool> TryAcquirePersistentLockAsync(DistributedLockCategory category, string key,
        TimeSpan lockDuration)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentNullException(nameof(key));

        var database = _connectionMultiplexer.GetDatabase();
        var lockKey = GetLockKey(category, key);

        try
        {
            var acquired = await database.StringSetAsync(lockKey, "1", lockDuration, When.NotExists);

            if (acquired)
            {
                _logger.Information("Persistent lock acquired: {Category}:{Key} for {Duration}", category, key,
                    lockDuration);
            }
            else
            {
                _logger.Information("Failed to acquire persistent lock: {Category}:{Key} (already locked)", category,
                    key);
            }

            return acquired;
        }
        catch (RedisConnectionException ex)
        {
            _logger.Error(ex, "Redis connection error while acquiring persistent lock: {Category}:{Key}", category,
                key);
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error acquiring persistent lock: {Category}:{Key}", category, key);
            throw;
        }
    }

    public bool IsLockAcquired(DistributedLockCategory category, string key)
    {
        var database = _connectionMultiplexer.GetDatabase();
        var lockKey = GetLockKey(category, key);

        return database.StringGet(lockKey) != RedisValue.Null;
    }

    public async Task<bool> IsLockAcquiredAsync(DistributedLockCategory category, string key)
    {
        var database = _connectionMultiplexer.GetDatabase();
        var lockKey = GetLockKey(category, key);

        return await database.StringGetAsync(lockKey) != RedisValue.Null;
    }

    public async Task<bool> ForceReleaseLockAsync(DistributedLockCategory category, string key)
    {
        var database = _connectionMultiplexer.GetDatabase();
        var lockKey = GetLockKey(category, key);

        try
        {
            return await database.KeyDeleteAsync(lockKey);
        }
        catch (RedisConnectionException ex)
        {
            _logger.Error(ex,
                "Redis connection error occurred while trying to force release a lock for category: {LockCategory}, name: {LockName}",
                category, lockKey);

            throw;
        }
        catch (Exception ex)
        {
            _logger.Error(ex,
                "An error occurred while trying to force release a lock for category: {LockCategory}, name: {LockName}",
                category, lockKey);

            throw;
        }
    }

    public async Task<TimeSpan?> GetLockTtlAsync(DistributedLockCategory category, string key)
    {
        var database = _connectionMultiplexer.GetDatabase();
        var lockKey = GetLockKey(category, key);

        try
        {
            var ttl = await database.KeyTimeToLiveAsync(lockKey);
            return ttl;
        }
        catch (RedisConnectionException ex)
        {
            _logger.Error(ex,
                "Redis connection error occurred while trying to get TTL for lock: {LockCategory}:{LockName}",
                category, lockKey);

            throw;
        }
        catch (Exception ex)
        {
            _logger.Error(ex,
                "An error occurred while trying to get TTL for lock: {LockCategory}:{LockName}",
                category, lockKey);

            throw;
        }
    }

    private string GetLockKey(DistributedLockCategory category, string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentNullException(nameof(key));

        return $"lock:{category}:{key}";
    }
}