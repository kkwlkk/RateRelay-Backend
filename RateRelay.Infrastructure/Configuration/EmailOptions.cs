namespace RateRelay.Infrastructure.Configuration;

public class EmailOptions
{
    public const string SectionName = "Email";

    public required bool Enabled { get; init; } = false;
    public required string SmtpHost { get; init; } = string.Empty;
    public required int SmtpPort { get; init; } = 587;
    public required string Username { get; init; } = string.Empty;
    public required string Password { get; init; } = string.Empty;
    public required string FromName { get; init; } = string.Empty;
}