using RateRelay.Domain.Enums;
using Serilog;
using Serilog.Events;

namespace RateRelay.Infrastructure.Extensions;

public static class LoggerExtensions
{
    public static void Verbose(this ILogger logger, LogPrefix prefix, string messageTemplate, params object[] propertyValues)
        => LogWithPrefix(logger, LogEventLevel.Verbose, prefix, messageTemplate, propertyValues);

    public static void Debug(this ILogger logger, LogPrefix prefix, string messageTemplate, params object[] propertyValues)
        => LogWithPrefix(logger, LogEventLevel.Debug, prefix, messageTemplate, propertyValues);

    public static void Information(this ILogger logger, LogPrefix prefix, string messageTemplate, params object[] propertyValues)
        => LogWithPrefix(logger, LogEventLevel.Information, prefix, messageTemplate, propertyValues);

    public static void Warning(this ILogger logger, LogPrefix prefix, string messageTemplate, params object[] propertyValues)
        => LogWithPrefix(logger, LogEventLevel.Warning, prefix, messageTemplate, propertyValues);

    public static void Error(this ILogger logger, LogPrefix prefix, string messageTemplate, params object[] propertyValues)
        => LogWithPrefix(logger, LogEventLevel.Error, prefix, messageTemplate, propertyValues);

    public static void Error(this ILogger logger, LogPrefix prefix, Exception exception, string messageTemplate, params object[] propertyValues)
        => LogWithPrefix(logger, LogEventLevel.Error, prefix, exception, messageTemplate, propertyValues);

    public static void Fatal(this ILogger logger, LogPrefix prefix, string messageTemplate, params object[] propertyValues)
        => LogWithPrefix(logger, LogEventLevel.Fatal, prefix, messageTemplate, propertyValues);

    public static void Fatal(this ILogger logger, LogPrefix prefix, Exception exception, string messageTemplate, params object[] propertyValues)
        => LogWithPrefix(logger, LogEventLevel.Fatal, prefix, exception, messageTemplate, propertyValues);

    private static void LogWithPrefix(ILogger logger, LogEventLevel logLevel, LogPrefix prefix, string messageTemplate, params object[] propertyValues)
    {
        using (Serilog.Context.LogContext.PushProperty("Prefix", prefix.ToString()))
        {
            logger.Write(logLevel, messageTemplate, propertyValues);
        }
    }

    private static void LogWithPrefix(ILogger logger, LogEventLevel logLevel, LogPrefix prefix, Exception exception, string messageTemplate, params object[] propertyValues)
    {
        using (Serilog.Context.LogContext.PushProperty("Prefix", prefix.ToString()))
        {
            logger.Write(logLevel, exception, messageTemplate, propertyValues);
        }
    }
}