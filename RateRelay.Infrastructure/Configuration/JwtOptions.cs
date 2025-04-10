using System.ComponentModel.DataAnnotations;
using RateRelay.Infrastructure.Attributes;

namespace RateRelay.Infrastructure.Configuration;

public class JwtOptions
{
    public const string SectionName = "Jwt";
    
    [Required(ErrorMessage = "Issuer is required")]
    public string Issuer { get; set; } 
    
    [Required(ErrorMessage = "Audience is required")]
    public string Audience { get; set; }
    
    [Required(ErrorMessage = "Secret is required")]
    public string Secret { get; set; }
    
    [Required(ErrorMessage = "Expiration is required")]
    [TimeSpanRange("01:00:00", "7.00:00:00", ErrorMessage = "Expiration must be between 1 hour and 7 days")]
    public TimeSpan Expiration { get; set; }

    [Required(ErrorMessage = "RefreshExpiration is required")]
    [TimeSpanRange("1.00:00:00", "14.00:00:00", ErrorMessage = "RefreshExpiration must be between 1 day and 14 days")]
    public TimeSpan RefreshExpiration { get; set; }
}