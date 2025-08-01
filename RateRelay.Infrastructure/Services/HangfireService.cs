using System.Linq.Expressions;
using System.Reflection;
using Hangfire;
using Hangfire.Redis.StackExchange;
using Microsoft.Extensions.Configuration;
using RateRelay.Domain.Enums;
using RateRelay.Infrastructure.Configuration;
using RateRelay.Infrastructure.Extensions;
using RateRelay.Infrastructure.Hangfire;
using RateRelay.Infrastructure.Hangfire.Filters;
using RateRelay.Infrastructure.Logging;
using Serilog;

namespace RateRelay.Infrastructure.Services;

public class HangfireService : IDisposable
{
    private readonly BackgroundJobServer _backgroundJobServer;
    private readonly IScopedLogger _logger;
    private static IServiceProvider _serviceProvider = null!;

    public HangfireService(IConfiguration configuration, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = LoggerFactory.CreateHangfireLogger(Log.Logger);

        var hangfireOptions = configuration.GetSection(HangfireOptions.SectionName).Get<HangfireOptions>();

        if (hangfireOptions is null)
            throw new ArgumentNullException(nameof(hangfireOptions));

        var connectionString = configuration.GetRedisConnectionString();

        GlobalConfiguration.Configuration.UseRedisStorage(connectionString, new RedisStorageOptions
        {
            Prefix = hangfireOptions.Prefix
        });

        GlobalConfiguration.Configuration.UseSerilogLogProvider();
        GlobalConfiguration.Configuration.UseFilter(new JobLoggingFilter());

        GlobalConfiguration.Configuration.UseActivator(new HangfireJobActivator(serviceProvider));

        _backgroundJobServer = new BackgroundJobServer(new BackgroundJobServerOptions
        {
            ServerName = hangfireOptions.ServerName,
            WorkerCount = hangfireOptions.WorkerCount
        });
    }

    public async Task StartAsync()
    {
        _logger.Information("Starting Hangfire service...");
        HangfireCleanupExtensions.CleanupOrphanedJobs(_serviceProvider);
        await LoadJobs(_serviceProvider);
        _logger.Information("Hangfire service started successfully.");
    }

    private Task LoadJobs(IServiceProvider serviceProvider)
    {
        var jobCount = 0;
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                var attribute = type.GetCustomAttribute<HangfireRecurringJobAttribute>();
                if (attribute == null) continue;

                var instance = serviceProvider.GetService(type);
                if (instance == null)
                {
                    throw new InvalidOperationException($"Cannot resolve type {type.FullName} from DI.");
                }

                var executeMethod = type.GetMethod("ExecuteAsync", BindingFlags.Public | BindingFlags.Instance);
                if (executeMethod == null)
                {
                    throw new InvalidOperationException($"Type {type.FullName} does not have an ExecuteAsync method.");
                }

                var methodCall = Expression.Lambda<Action>(
                    Expression.Call(Expression.Constant(instance), executeMethod));

                var jobOptions = new RecurringJobOptions
                {
                    TimeZone = attribute.TimeZone
                };

                RecurringJob.AddOrUpdate(attribute.RecurringJobId, methodCall, attribute.CronExpression, jobOptions);

                jobCount++;
            }
        }

        _logger.Information("Loaded {JobCount} jobs.", jobCount);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _backgroundJobServer?.Dispose();
    }
}