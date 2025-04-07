using System.ComponentModel.DataAnnotations;

namespace RateRelay.Infrastructure.Configuration;

public class LoggingOptions
{
    public const string SectionName = "Logging";

    [Required(ErrorMessage = "Enable console logging is required.")]
    public required bool EnableConsoleLogging { get; init; } = true;
    
    [Required(ErrorMessage = "Enable file logging is required.")]
    public required bool EnableFileLogging { get; init; } = true;
    
    public required string LogDirectory { get; init; } = "./logs";

    public required string LogFileRollingInterval { get; init; } = "Day";
}