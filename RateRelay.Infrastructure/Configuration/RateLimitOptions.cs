using System.ComponentModel.DataAnnotations;

namespace RateRelay.Infrastructure.Configuration;

public class RateLimitOptions
{
    public const string SectionName = "RateLimit";

    [Required]
    public bool EnableRateLimiting { get; set; } = true;

    [Required]
    public int DefaultLimit { get; set; } = 100;

    [Required]
    public TimeSpan DefaultPeriod { get; set; } = TimeSpan.FromMinutes(1);

    [Required]
    public int GlobalLimit { get; set; } = 1000;

    [Required]
    public TimeSpan GlobalPeriod { get; set; } = TimeSpan.FromMinutes(1);
    
    [Required]
    public string ResponseHeaderRetryAfter { get; set; } = "Retry-After";
    
    [Required]
    public string ResponseHeaderLimit { get; set; } = "X-RateLimit-Limit";
    
    [Required]
    public string ResponseHeaderRemaining { get; set; } = "X-RateLimit-Remaining";
    
    [Required]
    public string ResponseHeaderReset { get; set; } = "X-RateLimit-Reset";
}