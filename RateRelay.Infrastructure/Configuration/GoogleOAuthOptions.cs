using System.ComponentModel.DataAnnotations;

namespace RateRelay.Infrastructure.Configuration;

public class GoogleOAuthOptions
{
    public const string SectionName = "GoogleOAuth";

    [Required(ErrorMessage = "ClientId is required")]
    public string ClientId { get; set; }
}