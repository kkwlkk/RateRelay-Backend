using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RateRelay.Domain.Enums;

public enum PointTransactionType
{
    [Display(Name = "Successful Business Review")]
    [Description("Points earned for reviewing a business")]
    BusinessReview = 2,

    // Debit transactions (remove points)

    // Internal transactions
    [Display(Name = "Manual Adjustment")]
    [Description("Points manually adjusted by an administrator")]
    ManualAdjustment = 901,

    [Display(Name = "System Adjustment")]
    [Description("Points adjusted by the system automatically")]
    System = 902,
}