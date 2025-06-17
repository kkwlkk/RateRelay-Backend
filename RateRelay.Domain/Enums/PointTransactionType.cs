using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RateRelay.Domain.Enums;

public enum PointTransactionType
{
    [Display(Name = "Review Accepted Reward")]
    [Description("Points earned by reviewer when business owner accepts their review")]
    ReviewAcceptedReward = 1,

    [Display(Name = "Review Submission Lock")]
    [Description("Points temporarily locked from business owner when review is submitted")]
    ReviewSubmissionLock = 2,

    [Display(Name = "Review Rejection Return")]
    [Description("Points returned to business owner when they reject a review")]
    ReviewRejectionReturn = 3,

    [Display(Name = "Google Maps Review Bonus")]
    [Description("Additional points earned by reviewer for submitting optional Google Maps review")]
    GoogleMapsReviewBonus = 4,

    [Display(Name = "Google Maps Review Lock")]
    [Description("Additional points temporarily locked from business owner for Google Maps review")]
    GoogleMapsReviewLock = 5,

    [Display(Name = "Google Maps Review Return")]
    [Description("Additional points returned to business owner when Google Maps review is rejected")]
    GoogleMapsReviewReturn = 6,

    [Display(Name = "Manual Adjustment")]
    [Description("Points manually adjusted by an administrator")]
    ManualAdjustment = 901,

    [Display(Name = "System Adjustment")]
    [Description("Points adjusted by the system automatically")]
    System = 902,
}