namespace RateRelay.Application.DTOs.User.Referral.Queries;

public class ReferralGoalOutputDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string GoalType { get; set; } = string.Empty;
    public int TargetValue { get; set; }
    public int RewardPoints { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
}