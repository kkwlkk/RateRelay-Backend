using System.ComponentModel;

namespace RateRelay.Domain.Constants;

public static class PointConstants
{
    [Description("Points earned for successfully reviewing a business (e.g., writing a review that is accepted by the business)")]
    public const int BusinessReviewPoints = 1;

    [Description("Minimum points required for a user to be eligible for business visibility in the business queue system")]
    public const int MinimumOwnerPointBalanceForBusinessVisibility = 1;
}