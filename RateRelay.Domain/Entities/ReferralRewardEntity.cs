using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RateRelay.Domain.Entities;

[Table("referral_rewards")]
[Index(nameof(ReferrerAccountId))]
[Index(nameof(ReferredAccountId))]
[Index(nameof(DateAwardedUtc))]
public class ReferralRewardEntity : BaseEntity
{
    public long ReferrerAccountId { get; set; }
    
    [ForeignKey("ReferrerAccountId")]
    public virtual AccountEntity Referrer { get; set; } = null!;

    public long ReferredAccountId { get; set; }
    
    [ForeignKey("ReferredAccountId")]
    public virtual AccountEntity Referred { get; set; } = null!;

    public long GoalId { get; set; }

    [ForeignKey("GoalId")]
    public virtual ReferralGoalEntity Goal { get; set; } = null!;

    public int RewardPoints { get; set; }

    public DateTime DateAwardedUtc { get; set; } = DateTime.UtcNow;
}