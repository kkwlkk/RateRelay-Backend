using Serilog;
using RateRelay.Infrastructure.Constants;

namespace RateRelay.Infrastructure.Extensions;

public static class LoggerExtensions
{
    public static void LogWithPrefix(this ILogger logger, string prefix, string message, params object[] args)
    {
        if (LoggingConstants.PrefixColorMapping.TryGetValue(prefix, out var color) && IsConsoleAvailable())
        {
            LogWithColor(logger, prefix, color, message, args);
        }
        else
        {
            logger.Information("[{Prefix}] {Message}", prefix, string.Format(message, args));
        }
    }

    public static void LogWithColor(this ILogger logger, string prefix, ConsoleColor color, string message,
        params object[] args)
    {
        if (IsConsoleAvailable())
        {
            var originalColor = Console.ForegroundColor;

            try
            {
                Console.ForegroundColor = color;
                Console.Write($"[{prefix}] ");
                Console.ForegroundColor = originalColor;

                logger.Information(message, args);
            }
            finally
            {
                Console.ForegroundColor = originalColor;
            }
        }
        else
        {
            logger.Information("[{Prefix}] {Message}", prefix, string.Format(message, args));
        }
    }

    private static bool IsConsoleAvailable()
    {
        try
        {
            _ = Console.ForegroundColor;
            return true;
        }
        catch
        {
            return false;
        }
    }
}