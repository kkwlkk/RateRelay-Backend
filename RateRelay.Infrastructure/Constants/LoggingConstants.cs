namespace RateRelay.Infrastructure.Constants;

public static class LoggingConstants
{
    public static readonly Dictionary<string, ConsoleColor> PrefixColorMapping = new()
    {
        { "API", ConsoleColor.DarkBlue },
    };
        
    public const string ConsoleOutputTemplate = 
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";
            
    public const string FileOutputTemplate = 
        "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";
}