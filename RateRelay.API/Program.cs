using RateRelay.Application.DependencyInjection;
using RateRelay.Infrastructure.Configuration;
using RateRelay.Infrastructure.Environment;
using RateRelay.Infrastructure.Logging;
using Serilog;

namespace RateRelay.API;

public static class Program
{
    public static void Main(string[] args)
    {
        var environment = EnvironmentService.ConfigureEnvironment();
        
        var configuration = LoadConfiguration(args, environment);

        Log.Logger = LoggingConfiguration.CreateLoggerConfiguration(configuration).CreateLogger();

        try
        {
            Log.Information("Starting web host in {Environment} environment", environment);
            var host = CreateHostBuilder(args, environment).Build();
            host.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
    
    private static IConfiguration LoadConfiguration(string[] args, ApplicationEnvironment environment)
    {
        return new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile(environment.GetSettingsFileName(), optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();
    }

    private static IHostBuilder CreateHostBuilder(string[] args, ApplicationEnvironment environment) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureWebHostDefaults(webBuilder => 
            { 
                webBuilder.UseStartup<Startup>(); 
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton(environment);
                services.ConfigureOptions(hostContext.Configuration);
                services.ConfigureServices(hostContext.Configuration);
            })
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddJsonFile(environment.GetSettingsFileName(), optional: true, reloadOnChange: true);
                config.AddEnvironmentVariables();
                config.AddCommandLine(args);
            });
}