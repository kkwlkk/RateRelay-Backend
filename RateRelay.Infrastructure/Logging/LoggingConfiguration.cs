using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RateRelay.Infrastructure.Constants;
using Serilog;
using Serilog.Events;

namespace RateRelay.Infrastructure.Logging
{
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
            var logsDirectory = configuration["Logging:LogsDirectory"] ?? "./logs";
            var rollingInterval = Enum.Parse<RollingInterval>(
                configuration["Logging:RollingInterval"] ?? "Day");

            return new LoggerConfiguration()
                .MinimumLevel.Debug()
                .ConfigureMinimumLevels()
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

        private static LoggerConfiguration ConfigureEnrichment(this LoggerConfiguration loggerConfiguration)
        {
            return loggerConfiguration
                .Enrich.FromLogContext();
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
    }
}