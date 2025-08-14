using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using DotNetEnv;
using Newtonsoft.Json;
using RateRelay.Application.DependencyInjection;
using RateRelay.Domain.Interfaces;
using RateRelay.Infrastructure.Environment;
using RateRelay.Infrastructure.Logging;
using RateRelay.Infrastructure.Services;
using Serilog;

namespace RateRelay.API;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var environment = EnvironmentService.ConfigureEnvironment();

        var configuration = LoadConfiguration(args, environment);
        
        Log.Logger.Debug("Configuration: {Configuration}", JsonConvert.SerializeObject(configuration, Formatting.Indented));
        Log.Logger.Debug($"JWT Secret Key: {configuration["Jwt:Secret"]}");

        Log.Logger = LoggingConfiguration.CreateLoggerConfiguration(configuration).CreateLogger();

        try
        {
            Log.Information("Starting web host in {Environment} environment", environment);
            var host = CreateHostBuilder(args, environment).Build();

            using (var scope = host.Services.CreateScope())
            {
                var success = await PerformMigrationAsync(scope.ServiceProvider);
                if (!success)
                {
                    throw new InvalidOperationException("Database migration failed");
                }
            }

            using (var hangfireService = new HangfireService(configuration, host.Services))
            {
                await hangfireService.StartAsync();
            }

            await host.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    private static IConfiguration LoadConfiguration(string[] args, ApplicationEnvironment environment)
    {
        var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                       ?? AppContext.BaseDirectory;

        if (!environment.IsProduction)
        {
            Env.LoadMulti([
                Path.Combine(basePath, ".env"),
                Path.Combine(basePath, ".env.local"),
                Path.Combine(basePath, $".env.{environment.Name}")
            ]);
        }

        var builder = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile(environment.GetSettingsFileName(), optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args);

        return builder.Build();
    }

    private static IHostBuilder CreateHostBuilder(string[] args, ApplicationEnvironment environment) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
            .ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton(environment);
                services.ConfigureOptions(hostContext.Configuration);
                services.ConfigureServices(hostContext.Configuration);
            });

    private static async Task<bool> PerformMigrationAsync(IServiceProvider serviceProvider)
    {
        var migrationService = serviceProvider.GetRequiredService<MigrationService>();
        return await migrationService.UpdateDatabaseAsync();
    }
}