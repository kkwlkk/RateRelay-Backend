using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RateRelay.Domain.Entities;

[Table("account_bans")]
public class AccountBanEntity : BaseEntity
{
    public long AccountId { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }

    [MaxLength(255)]
    public required string Reason { get; set; }

    public bool IsActive => ExpiresAtUtc is null || ExpiresAtUtc > DateTime.UtcNow;

    public bool IsExpired => ExpiresAtUtc is not null && ExpiresAtUtc <= DateTime.UtcNow;

    // navigation
    public virtual AccountEntity Account { get; set; } = null!;
}