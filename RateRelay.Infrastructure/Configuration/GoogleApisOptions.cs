using System.ComponentModel.DataAnnotations;

namespace RateRelay.Infrastructure.Configuration;

public class GoogleApisOptions
{
    public const string SectionName = "GoogleApis";
    
    [Required(ErrorMessage = "Google API key is required.")]
    public required string ApiKey { get; init; }
}