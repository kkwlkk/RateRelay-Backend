using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RateRelay.Infrastructure.Configuration;
using RateRelay.Infrastructure.Constants;
using RateRelay.Infrastructure.Logging.Enrichers;
using Serilog;
using Serilog.Events;

namespace RateRelay.Infrastructure.Logging;

public static class LoggingConfiguration
{
    public static IServiceCollection AddLogging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var loggerConfiguration = CreateLoggerConfiguration(configuration);

        Log.Logger = loggerConfiguration.CreateLogger();

        services.AddSingleton(Log.Logger);

        return services;
    }

    public static LoggerConfiguration CreateLoggerConfiguration(IConfiguration configuration)
    {
        const string sectionName = SerilogLoggingOptions.SectionName;
        var logsDirectory = configuration[$"{sectionName}:LogDirectory"] ?? "./logs";

        var assemblyLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        if (string.IsNullOrEmpty(assemblyLocation))
            throw new InvalidOperationException("Could not determine assembly location");

        logsDirectory = Path.GetFullPath(Path.Combine(assemblyLocation, logsDirectory));
        Directory.CreateDirectory(logsDirectory);

        var rollingIntervalConfig = configuration[$"{sectionName}:LogFileRollingInterval"] ?? "Day";
        if (!Enum.TryParse<RollingInterval>(rollingIntervalConfig, true, out var rollingInterval))
            throw new InvalidOperationException($"Invalid rolling interval: {rollingIntervalConfig}");

        return new LoggerConfiguration()
            .MinimumLevel.Debug()
            .ConfigureMinimumLevels()
            .ConfigureFilters()
            .ConfigureEnrichment()
            .ConfigureOutputSinks(logsDirectory, rollingInterval);
    }

    private static LoggerConfiguration ConfigureMinimumLevels(this LoggerConfiguration loggerConfiguration)
    {
        return loggerConfiguration
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Hangfire", LogEventLevel.Warning);
    }

    private static LoggerConfiguration ConfigureFilters(this LoggerConfiguration loggerConfiguration)
    {
        return loggerConfiguration
            .Filter.ByExcluding(logEvent =>
                logEvent.Properties.TryGetValue("RequestPath", out var requestPath) &&
                requestPath.ToString().Contains("/hangfire/") &&
                logEvent.Level == LogEventLevel.Information);
    }

    private static LoggerConfiguration ConfigureOutputSinks(
        this LoggerConfiguration loggerConfiguration,
        string logsDirectory,
        RollingInterval rollingInterval)
    {
        return loggerConfiguration
            .WriteTo.Console(
                outputTemplate: LoggingConstants.ConsoleOutputTemplate
            )
            .WriteTo.File(
                $"{logsDirectory}/.log",
                rollingInterval: rollingInterval,
                rollOnFileSizeLimit: true,
                outputTemplate: LoggingConstants.FileOutputTemplate
            );
    }

    private static LoggerConfiguration ConfigureEnrichment(this LoggerConfiguration loggerConfiguration)
    {
        return loggerConfiguration
            .Enrich.FromLogContext()
            .Enrich.With<ClientIpEnricher>()
            .Enrich.With(new PrefixFormatterEnricher(LoggingConstants.PrefixColorMapping));
    }
}