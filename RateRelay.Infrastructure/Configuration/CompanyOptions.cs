namespace RateRelay.Infrastructure.Configuration;

public class CompanyOptions
{
    public const string SectionName = "Company";
    
    public required string Name { get; init; } = string.Empty;
    public required string LogoUrl { get; init; } = string.Empty;
    public int LogoWidth { get; init; } = 240;
    public int LogoHeight { get; init; } = 90;
    public string Address { get; init; } = string.Empty;
    public string Website { get; init; } = string.Empty;
    public EmailsConfig Emails { get; init; } = new();
    public List<SocialLinkConfig> SocialLinks { get; init; } = [];
}

public class EmailsConfig
{
    public string SupportEmail { get; init; } = string.Empty;
    public string PrivacyEmail { get; init; } = string.Empty;
}

public class SocialLinkConfig
{
    public string Platform { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
}