using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using RateRelay.Domain.Enums;

namespace RateRelay.Domain.Entities;

[Table("accounts")]
[Index(nameof(Email), IsUnique = true)]
[Index(nameof(GoogleId), IsUnique = true)]
[Index(nameof(Username), IsUnique = true)]
public class AccountEntity : BaseEntity
{
    [MaxLength(64)]
    public required string Username { get; set; }

    [MaxLength(255)]
    public required string Email { get; set; }

    [MaxLength(255)]
    public required string GoogleId { get; set; }
    public ulong Permissions { get; set; }
    
    public int PointBalance { get; set; }

    public long? RoleId { get; set; }

    [ForeignKey("RoleId")]
    public virtual RoleEntity? Role { get; set; }
    
    public AccountOnboardingStep OnboardingStep { get; set; } = AccountOnboardingStep.Welcome;
    
    public DateTime? OnboardingLastUpdatedUtc { get; set; }
    
    [NotMapped]
    public bool HasCompletedOnboarding => OnboardingStep == AccountOnboardingStep.Completed;
    
    
}