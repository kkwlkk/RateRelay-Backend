using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RateRelay.Domain.Interfaces;
using RateRelay.Domain.Interfaces.DataAccess;
using RateRelay.Domain.Interfaces.DataAccess.Redis;
using RateRelay.Infrastructure.Configuration;
using RateRelay.Infrastructure.DataAccess.Context;
using RateRelay.Infrastructure.DataAccess.Redis;
using RateRelay.Infrastructure.DataAccess.Repositories;
using RateRelay.Infrastructure.DataAccess.UnitOfWork;
using RateRelay.Infrastructure.Services;
using StackExchange.Redis;

namespace RateRelay.Infrastructure.Extensions;

public static class DataAccessExtensions
{
    public static void AddRateRelayDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetDatabaseConnectionString();

        services.AddDbContextFactory<RateRelayDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
                builder => { }));

        services.AddSingleton<MigrationService>();
        services.AddSingleton<IUnitOfWorkFactory, UnitOfWorkFactory>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
    }

    public static void AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetRedisConnectionString();

        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(connectionString));
        services.AddScoped<IRedisCacheProvider, RedisCacheProvider>();
        services.AddScoped<IRedisDistributedLockProvider, RedisDistributedLockProvider>();
    }

    private static string GetDatabaseConnectionString(this IConfiguration configuration)
    {
        var databaseOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>();

        if (databaseOptions is null)
            throw new ArgumentNullException(nameof(databaseOptions));

        return $"{databaseOptions.ConnectionString};password={databaseOptions.Password}";
    }

    private static string GetRedisConnectionString(this IConfiguration configuration)
    {
        var redisOptions = configuration.GetSection(RedisOptions.SectionName).Get<RedisOptions>();

        if (redisOptions is null)
            throw new ArgumentNullException(nameof(redisOptions));

        return $"{redisOptions.ConnectionString},password={redisOptions.Password}";
    }
}