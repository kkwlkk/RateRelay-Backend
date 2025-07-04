namespace RateRelay.Application.DTOs.User.Referral;

public class ReferralRewardDto
{
    public string GoalName { get; set; } = string.Empty;
    public int RewardPoints { get; set; }
    public DateTime DateAwarded { get; set; }
    public string ReferredUserName { get; set; } = string.Empty;
}