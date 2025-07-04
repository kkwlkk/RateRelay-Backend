namespace RateRelay.Application.DTOs.User.Referral;

public class ReferralProgressDto
{
    public string GoalName { get; set; } = string.Empty;
    public string GoalDescription { get; set; } = string.Empty;
    public int TargetValue { get; set; }
    public int CurrentValue { get; set; }
    public int RewardPoints { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? DateCompleted { get; set; }
    public string ReferredUserName { get; set; } = string.Empty;
    public decimal ProgressPercentage => TargetValue > 0 ? Math.Min(100, (decimal)CurrentValue / TargetValue * 100) : 0;
}