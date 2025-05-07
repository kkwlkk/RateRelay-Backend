using System.Text.Json;
using RateRelay.Domain.Enums.Redis;
using RateRelay.Domain.Interfaces.DataAccess.Redis;
using Serilog;
using StackExchange.Redis;

namespace RateRelay.Infrastructure.DataAccess.Redis;

public class RedisCacheProvider(IConnectionMultiplexer connectionMultiplexer, ILogger logger) : IRedisCacheProvider
{
    private readonly IConnectionMultiplexer _connectionMultiplexer =
        connectionMultiplexer ?? throw new ArgumentNullException(nameof(connectionMultiplexer));

    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private static string GetCacheKey(CacheEntryCategory category, string key)
    {
        return string.Concat("cache:", category, ":", key);
    }


    public async Task SetAsync<T>(CacheEntryCategory category, string key, T value, TimeSpan? expiry = null)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentNullException(nameof(key));

        if (value is null)
            throw new ArgumentNullException(nameof(value));

        var database = _connectionMultiplexer.GetDatabase();
        var cacheKey = GetCacheKey(category, key);

        try
        {
            var serializedValue = JsonSerializer.Serialize(value);
            await database.StringSetAsync(cacheKey, serializedValue, expiry);
            _logger.Information("Set cache entry with key: {CacheKey} and expiry: {Expiry}", cacheKey, expiry);
        }
        catch (RedisConnectionException ex)
        {
            _logger.Error(ex,
                "Redis connection error occured while setting cache entry with key: {CacheKey}, value: {Value}, expiry: {Expiry}",
                cacheKey, value, expiry);

            throw;
        }
        catch (Exception ex)
        {
            _logger.Error(ex,
                "An error occured while setting cache entry with key: {CacheKey}, value: {Value}, expiry: {Expiry}",
                cacheKey, value, expiry);

            throw;
        }
    }

    public async Task<T?> GetAsync<T>(CacheEntryCategory category, string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentNullException(nameof(key));

        var database = _connectionMultiplexer.GetDatabase();
        var cacheKey = GetCacheKey(category, key);

        try
        {
            var cachedValue = await database.StringGetAsync(cacheKey);

            if (cachedValue.IsNullOrEmpty)
            {
                _logger.Information("Cache miss for key: {CacheKey}", cacheKey);
                return default;
            }

            var deserializedValue = JsonSerializer.Deserialize<T>(cachedValue!);
            _logger.Information("Cache hit for key: {CacheKey}", cacheKey);

            return deserializedValue;
        }
        catch (RedisConnectionException ex)
        {
            _logger.Error(ex, "Redis connection error occured while getting cache entry with key: {CacheKey}",
                cacheKey);

            throw;
        }
        catch (Exception ex)
        {
            _logger.Error(ex,
                "An error occured while getting cache entry with key: {CacheKey}", cacheKey);

            throw;
        }
    }

    public async Task<IEnumerable<T?>> GetAllAsync<T>(CacheEntryCategory category)
    {
        if (string.IsNullOrWhiteSpace(category.ToString()))
            throw new ArgumentNullException(nameof(category));

        var database = _connectionMultiplexer.GetDatabase();

        try
        {
            var server = _connectionMultiplexer.GetServer(_connectionMultiplexer.GetEndPoints()[0]);
            var keys = server.Keys(pattern: $"cache:{category}:*").Select(k => k.ToString()).ToArray();

            var tasks = keys.Select(k => database.StringGetAsync(k));
            var results = await Task.WhenAll(tasks);

            return results
                .Where(r => !r.IsNullOrEmpty)
                .Select(r => JsonSerializer.Deserialize<T>(r!))
                .ToList();
        }
        catch (RedisConnectionException ex)
        {
            _logger.Error(ex,
                "Redis connection error occured while getting all cache entries with category: {CacheCategory}",
                category);

            throw;
        }
        catch (Exception ex)
        {
            _logger.Error(ex,
                "An error occured while getting all cache entries with category: {CacheCategory}", category);

            throw;
        }
    }


    public async Task<bool> RemoveAsync(CacheEntryCategory category, string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentNullException(nameof(key));

        var database = _connectionMultiplexer.GetDatabase();
        var cacheKey = GetCacheKey(category, key);

        try
        {
            var result = await database.KeyDeleteAsync(cacheKey);

            if (result)
            {
                _logger.Information("Cache entry removed with key: {CacheKey}", cacheKey);
            }
            else
            {
                _logger.Warning("Failed to remove cache entry with key: {CacheKey} or key not found", cacheKey);
            }

            return result;
        }
        catch (RedisConnectionException ex)
        {
            _logger.Error(ex, "Redis connection error occurred while removing cache: {CacheKey}", cacheKey);

            throw;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "An error occurred while removing cache: {CacheKey}", cacheKey);

            throw;
        }
    }

    public Task<bool> KeyExistsAsync(CacheEntryCategory category, string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentNullException(nameof(key));

        var database = _connectionMultiplexer.GetDatabase();
        var cacheKey = GetCacheKey(category, key);

        try
        {
            return database.KeyExistsAsync(cacheKey);
        }
        catch (RedisConnectionException ex)
        {
            _logger.Error(ex, "Redis connection error occurred while checking key existence: {CacheKey}", cacheKey);

            throw;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "An error occurred while checking key existence: {CacheKey}", cacheKey);

            throw;
        }
    }
}