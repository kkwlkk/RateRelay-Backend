namespace RateRelay.Application.DTOs.User.Referral.Queries;

public class ReferralStatsOutputDto
{
    public int TotalReferrals { get; set; }
    public int ActiveReferrals { get; set; }
    public int CompletedGoals { get; set; }
    public int TotalPointsEarned { get; set; }
    public int PendingRewards { get; set; }
    public string ReferralCode { get; set; } = string.Empty;
    public string? ReferredByCode { get; set; } = string.Empty;
    public List<ReferralProgressDto> Progress { get; set; } = [];
    public List<ReferralRewardDto> RecentRewards { get; set; } = [];
}