using System.ComponentModel.DataAnnotations;

namespace RateRelay.Infrastructure.Configuration;

public class EncryptionOptions
{
    public const string SectionName = "Encryption";

    /// <summary>
    /// The encryption key used for encrypting and decrypting sensitive data.
    /// </summary>
    [Required(ErrorMessage = "Encryption key is required.")]
    public required string Key { get; init; } = string.Empty;
}