using System.ComponentModel.DataAnnotations;

namespace RateRelay.Infrastructure.Configuration;

public record RedisOptions
{
    public const string SectionName = "Redis";

    [Required(ErrorMessage = "Redis connection string is required.")]
    public required string ConnectionString { get; init; }

    [Required(ErrorMessage = "Redis password is required.")]
    public required string Password { get; init; }
}