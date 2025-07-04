using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RateRelay.Domain.Enums;

public enum ReferralGoalType
{
    [Display(Name = "Reviews Completed")]
    [Description("User completes a certain number of business reviews")]
    ReviewsCompleted = 0,

    [Display(Name = "Business Verified")]
    [Description("User verifies their business")]
    BusinessVerified = 1,

    [Display(Name = "Points Earned")]
    [Description("User earns a certain number of points")]
    PointsEarned = 2,

    [Display(Name = "Onboarding Completed")]
    [Description("User completes the onboarding process")]
    OnboardingCompleted = 3
}