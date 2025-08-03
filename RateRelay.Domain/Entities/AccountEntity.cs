using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using RateRelay.Domain.Enums;

namespace RateRelay.Domain.Entities;

[Table("accounts")]
[Index(nameof(Email), IsUnique = true)]
[Index(nameof(GoogleId), IsUnique = true)]
[Index(nameof(GoogleUsername), IsUnique = true)]
public class AccountEntity : BaseEntity
{
    [MaxLength(64)]
    [Description("Immutable Google username, used for system purposes and can be changed only by system or administrators.")]
    public required string GoogleUsername { get; set; }

    [MaxLength(255)]
    public required string Email { get; set; }

    [MaxLength(255)]
    public required string GoogleId { get; set; }

    [MaxLength(16)]
    public string? ReferralCode { get; set; }

    public long? ReferredByAccountId { get; set; }

    [ForeignKey("ReferredByAccountId")]
    public virtual AccountEntity? ReferredBy { get; set; }

    public ulong Permissions { get; set; }

    public int PointBalance { get; set; }

    public long? RoleId { get; set; }

    [ForeignKey("RoleId")]
    public virtual RoleEntity? Role { get; set; }

    public AccountOnboardingStep OnboardingStep { get; set; } = AccountOnboardingStep.BusinessVerification;

    public DateTime? OnboardingLastUpdatedUtc { get; set; }

    public AccountFlags Flags { get; set; }

    [NotMapped]
    public bool HasCompletedOnboarding => OnboardingStep == AccountOnboardingStep.Completed;

    [InverseProperty("ReferredBy")]
    public virtual ICollection<AccountEntity> Referrals { get; set; } = new List<AccountEntity>();

    [InverseProperty("Referrer")]
    public virtual ICollection<ReferralProgressEntity> ReferralProgress { get; set; } =
        new List<ReferralProgressEntity>();

    [InverseProperty("Referred")]
    public virtual ICollection<ReferralProgressEntity> ReferredProgress { get; set; } =
        new List<ReferralProgressEntity>();

    [InverseProperty("Referrer")]
    public virtual ICollection<ReferralRewardEntity> ReferralRewards { get; set; } = new List<ReferralRewardEntity>();

    [InverseProperty("Referred")]
    public virtual ICollection<ReferralRewardEntity> ReceivedRewards { get; set; } = new List<ReferralRewardEntity>();
}