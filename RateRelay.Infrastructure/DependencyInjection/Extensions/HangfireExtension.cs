using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RateRelay.Infrastructure.Services;
using System.Reflection;
using RateRelay.Infrastructure.Hangfire;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.Redis.StackExchange;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using RateRelay.Domain.Enums;
using RateRelay.Infrastructure.Authorization;
using RateRelay.Infrastructure.Configuration;
using RateRelay.Infrastructure.Environment;
using RateRelay.Infrastructure.Extensions;

namespace RateRelay.Infrastructure.DependencyInjection.Extensions;

public static class HangfireExtension
{
    public static void AddHangfire(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<HangfireService>(sp =>
            new HangfireService(configuration, sp));

        services.AddHangfire(config =>
        {
            var connectionString = configuration.GetRedisConnectionString();
            var hangfireOptions = configuration.GetSection(HangfireOptions.SectionName).Get<HangfireOptions>();

            config.UseRedisStorage(connectionString, new RedisStorageOptions
            {
                Prefix = hangfireOptions?.Prefix ?? "hangfire"
            });
        });

        services.AddHangfireServer();

        RegisterHangfireJobs(services);
    }

    public static void UseHangfireDashboard(this IApplicationBuilder app)
    {
        var serviceProvider = app.ApplicationServices;
        var env = ApplicationEnvironment.Current();
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            AppPath = null,
            DashboardTitle = $"{(env.IsDevelopment ? "DEV" : "PROD")} - Background Jobs",
            StatsPollingInterval = env.IsDevelopment ? 1000 : 3000,
            DisplayStorageConnectionString = env.IsDevelopment,
            DefaultRecordsPerPage = 50,
            Authorization = [new HangfireAuthorizationFilter(serviceProvider)],
            IgnoreAntiforgeryToken = true,
            IsReadOnlyFunc = context => !HasManagePermission(context, serviceProvider),
        });
    }

    private static bool HasManagePermission(DashboardContext context, IServiceProvider serviceProvider)
    {
        var httpContext = context.GetHttpContext();

        if (httpContext.Request.IsLocal())
            return true;

        using var scope = serviceProvider.CreateScope();
        var currentUserContext = scope.ServiceProvider.GetRequiredService<CurrentUserContext>();

        return currentUserContext.IsAuthenticated &&
               currentUserContext.HasPermission(Permission.ManageHangfireJobs);
    }

    private static void RegisterHangfireJobs(IServiceCollection services)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => assembly.FullName?.StartsWith("RateRelay") == true);

        foreach (var assembly in assemblies)
        {
            var jobTypes = assembly.GetTypes()
                .Where(type => type.GetCustomAttribute<HangfireRecurringJobAttribute>() != null)
                .Where(type => type is { IsAbstract: false, IsInterface: false });

            foreach (var jobType in jobTypes)
            {
                services.AddTransient(jobType);
            }
        }
    }
}