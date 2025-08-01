using System.ComponentModel.DataAnnotations;

namespace RateRelay.Infrastructure.Configuration;

public record HangfireOptions
{
    public const string SectionName = "Hangfire";

    [Required(ErrorMessage = "Hangfire prefix is required.")]
    public required string Prefix { get; init; }

    [Required(ErrorMessage = "Hangfire server name is required.")]
    public required string ServerName { get; init; }

    [Required(ErrorMessage = "Hangfire worker count is required.")]
    public required int WorkerCount { get; init; }
}