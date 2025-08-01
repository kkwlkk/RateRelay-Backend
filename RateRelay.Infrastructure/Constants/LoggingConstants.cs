using RateRelay.Domain.Enums;
using RateRelay.Infrastructure.Logging;

namespace RateRelay.Infrastructure.Constants;

public static class LoggingConstants
{
    public static readonly Dictionary<string, string> PrefixColorMapping = new()
    {
        { "API", AnsiColor.DarkBlue },
        { LogPrefix.HANGFIRE.ToString(), AnsiColor.Blue },
    };

    public const string ConsoleOutputTemplate =
        "[{Timestamp:HH:mm:ss} {Level:u3}]{FormattedPrefixWithColor}{ClientIP} {Message:lj}{NewLine}{Exception}";

    public const string FileOutputTemplate =
        "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}]{FormattedPrefixPlain}{ClientIP} {Message:lj}{NewLine}{Exception}";
}