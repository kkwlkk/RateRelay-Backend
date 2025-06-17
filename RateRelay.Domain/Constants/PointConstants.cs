using System.ComponentModel;

namespace RateRelay.Domain.Constants;

public static class PointConstants
{
    [Description("Points deducted from business owner when a review is submitted (locked until review is processed)")]
    public const int ReviewSubmissionLockPoints = 1;
    
    [Description("Points awarded to reviewer when business owner accepts their review")]
    public const int AcceptedReviewRewardPoints = 1;
    
    [Description("Points returned to business owner when they reject a review (unlocks previously locked points)")]
    public const int RejectedReviewReturnPoints = 1;
    
    [Description("Minimum points required for a business owner to receive reviews and maintain platform visibility")]
    public const int MinimumOwnerPointBalanceForBusinessVisibility = 1;
}