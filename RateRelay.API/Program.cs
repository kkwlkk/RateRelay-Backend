using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
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

        if (!environment.IsProduction)
        {
            LoadEnvironmentVariables();
        }

        var configuration = LoadConfiguration(args, environment);

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
                webBuilder.ConfigureKestrel(options =>
                {
                    var configuration = LoadConfiguration(args, environment);
                    var certificateSettings = configuration.GetSection("Certificate");

                    var certificatePath = certificateSettings.GetValue<string>("Path");
                    var certificatePassword = certificateSettings.GetValue<string>("Password");

                    var httpPort = configuration.GetValue("Kestrel:HttpPort", 5000);
                    var httpsPort = configuration.GetValue("Kestrel:HttpsPort", 5001);

                    options.ListenAnyIP(httpPort,
                        listenOptions =>
                        {
                            listenOptions.Protocols =
                                Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
                        });

                    options.ListenAnyIP(httpsPort, listenOptions =>
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(certificatePath) && !string.IsNullOrEmpty(certificatePassword) &&
                                File.Exists(certificatePath))
                            {
                                try
                                {
                                    using var cert = new X509Certificate2(certificatePath, certificatePassword);
                                    listenOptions.UseHttps(certificatePath, certificatePassword);
                                    Log.Information("Using custom certificate for HTTPS on port {HttpsPort}",
                                        httpsPort);
                                }
                                catch (CryptographicException ex)
                                {
                                    Log.Error(ex,
                                        "Failed to load certificate from {CertificatePath} with the provided password",
                                        certificatePath);
                                    throw new InvalidOperationException(
                                        $"Cannot load certificate from {certificatePath}. Please check the password is correct.",
                                        ex);
                                }
                            }
                            else
                            {
                                listenOptions.UseHttps();
                                Log.Information("Using development certificate for HTTPS on port {HttpsPort}",
                                    httpsPort);
                            }

                            listenOptions.Protocols =
                                Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "Error configuring HTTPS");
                            throw;
                        }
                    });
                });
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

    private static async Task<bool> PerformMigrationAsync(IServiceProvider serviceProvider)
    {
        var migrationService = serviceProvider.GetRequiredService<MigrationService>();
        return await migrationService.UpdateDatabaseAsync();
    }

    private static void LoadEnvironmentVariables()
    {
        DotNetEnv.Env.Load();
    }
}