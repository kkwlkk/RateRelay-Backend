namespace RateRelay.Infrastructure.Configuration;

public class EmailLinksOptions
{
    public const string SectionName = "EmailLinks";

    public string Support { get; set; } = string.Empty;
    public string Unsubscribe { get; set; } = string.Empty;
    public string Preferences { get; set; } = string.Empty;
}