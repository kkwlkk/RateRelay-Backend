namespace RateRelay.Domain.Common.DTOs;

public class ReferralStatsDto
{
    public int TotalReferrals { get; set; }
    public int ActiveReferrals { get; set; }
    public int CompletedGoals { get; set; }
    public int TotalPointsEarned { get; set; }
    public int PendingRewards { get; set; }
    public string ReferralCode { get; set; } = string.Empty;
    public string? ReferredByCode { get; set; } = string.Empty;
    public List<ReferralProgressSummary> Progress { get; set; } = [];
}

public class ReferralProgressSummary
{
    public string GoalName { get; set; } = string.Empty;
    public string GoalDescription { get; set; } = string.Empty;
    public int TargetValue { get; set; }
    public int CurrentValue { get; set; }
    public int RewardPoints { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? DateCompleted { get; set; }
    public string ReferredUserName { get; set; } = string.Empty;
}