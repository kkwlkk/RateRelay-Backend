using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using RateRelay.Domain.Enums;

namespace RateRelay.Domain.Entities;

[Table("referral_goals")]
[Index(nameof(GoalType))]
[Index(nameof(IsActive))]
public class ReferralGoalEntity : BaseEntity
{
    [MaxLength(64)]
    public required string Name { get; set; }

    [MaxLength(255)]
    public required string Description { get; set; }

    public ReferralGoalType GoalType { get; set; }

    public int TargetValue { get; set; } = 1;

    public int RewardPoints { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; } = 0;

    [InverseProperty("Goal")]
    public virtual ICollection<ReferralProgressEntity> Progress { get; set; } = new List<ReferralProgressEntity>();

    [InverseProperty("Goal")]
    public virtual ICollection<ReferralRewardEntity> Rewards { get; set; } = new List<ReferralRewardEntity>();
}