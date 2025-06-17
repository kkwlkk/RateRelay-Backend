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

    [Display(Name = "Manual Adjustment")]
    [Description("Points manually adjusted by an administrator")]
    ManualAdjustment = 901,

    [Display(Name = "System Adjustment")]
    [Description("Points adjusted by the system automatically")]
    System = 902,
}