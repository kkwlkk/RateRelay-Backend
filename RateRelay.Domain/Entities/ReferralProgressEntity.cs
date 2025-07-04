using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RateRelay.Domain.Entities;

[Table("referral_progress")]
[Index(nameof(ReferrerAccountId))]
[Index(nameof(ReferredAccountId))]
[Index(nameof(GoalId))]
[Index(nameof(IsCompleted), nameof(DateCompletedUtc))]
public class ReferralProgressEntity : BaseEntity
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

    public int CurrentValue { get; set; } = 0;

    public bool IsCompleted { get; set; } = false;

    public DateTime? DateCompletedUtc { get; set; }

    public void UpdateProgress(int newValue)
    {
        CurrentValue = newValue;
        if (IsCompleted || CurrentValue < Goal.TargetValue) return;
        IsCompleted = true;
        DateCompletedUtc = DateTime.UtcNow;
    }
}