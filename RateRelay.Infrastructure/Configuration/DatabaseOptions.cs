using System.ComponentModel.DataAnnotations;

namespace RateRelay.Infrastructure.Configuration;

public record DatabaseOptions
{
    public const string SectionName = "Database";

    [Required(ErrorMessage = "Database connection string is required.")]
    public required string ConnectionString { get; init; }

    [Required(ErrorMessage = "Database password is required.")]
    public required string Password { get; init; }

    [Required(ErrorMessage = "Database migration timeout is required.")]
    public required TimeSpan MigrationTimeout { get; init; }
}
