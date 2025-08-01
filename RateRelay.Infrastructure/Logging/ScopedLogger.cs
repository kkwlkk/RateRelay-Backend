using RateRelay.Domain.Enums;
using Serilog;
using Serilog.Events;

namespace RateRelay.Infrastructure.Logging;

public interface IScopedLogger
{
    void Verbose(string messageTemplate, params object[] propertyValues);
    void Debug(string messageTemplate, params object[] propertyValues);
    void Information(string messageTemplate, params object[] propertyValues);
    void Warning(string messageTemplate, params object[] propertyValues);
    void Error(string messageTemplate, params object[] propertyValues);
    void Error(Exception exception, string messageTemplate, params object[] propertyValues);
    void Fatal(string messageTemplate, params object[] propertyValues);
    void Fatal(Exception exception, string messageTemplate, params object[] propertyValues);
}

public class ScopedLogger(ILogger logger, LogPrefix prefix) : IScopedLogger
{
    public void Verbose(string messageTemplate, params object[] propertyValues)
        => LogWithPrefix(LogEventLevel.Verbose, messageTemplate, propertyValues);

    public void Debug(string messageTemplate, params object[] propertyValues)
        => LogWithPrefix(LogEventLevel.Debug, messageTemplate, propertyValues);

    public void Information(string messageTemplate, params object[] propertyValues)
        => LogWithPrefix(LogEventLevel.Information, messageTemplate, propertyValues);

    public void Warning(string messageTemplate, params object[] propertyValues)
        => LogWithPrefix(LogEventLevel.Warning, messageTemplate, propertyValues);

    public void Error(string messageTemplate, params object[] propertyValues)
        => LogWithPrefix(LogEventLevel.Error, messageTemplate, propertyValues);

    public void Error(Exception exception, string messageTemplate, params object[] propertyValues)
        => LogWithPrefix(LogEventLevel.Error, exception, messageTemplate, propertyValues);

    public void Fatal(string messageTemplate, params object[] propertyValues)
        => LogWithPrefix(LogEventLevel.Fatal, messageTemplate, propertyValues);

    public void Fatal(Exception exception, string messageTemplate, params object[] propertyValues)
        => LogWithPrefix(LogEventLevel.Fatal, exception, messageTemplate, propertyValues);

    private void LogWithPrefix(LogEventLevel logLevel, string messageTemplate, params object[] propertyValues)
    {
        using (Serilog.Context.LogContext.PushProperty("Prefix", prefix.ToString()))
        {
            logger.Write(logLevel, messageTemplate, propertyValues);
        }
    }

    private void LogWithPrefix(LogEventLevel logLevel, Exception exception, string messageTemplate,
        params object[] propertyValues)
    {
        using (Serilog.Context.LogContext.PushProperty("Prefix", prefix.ToString()))
        {
            logger.Write(logLevel, exception, messageTemplate, propertyValues);
        }
    }
}

public static class LoggerFactory
{
    public static IScopedLogger CreateHangfireLogger(ILogger baseLogger)
        => new ScopedLogger(baseLogger, LogPrefix.HANGFIRE);
}