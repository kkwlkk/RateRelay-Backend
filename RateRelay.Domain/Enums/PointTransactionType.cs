using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RateRelay.Domain.Enums;

public enum PointTransactionType
{
    // Credit transactions (add points)
    [Display(Name = "Successful Business Verification")]
    [Description("Points earned for verifying a business for the first time")]
    BusinessVerification = 1,

    // Debit transactions (remove points)

    // Internal transactions
    [Display(Name = "Manual Adjustment")]
    [Description("Points manually adjusted by an administrator")]
    ManualAdjustment = 901,

    [Display(Name = "System Adjustment")]
    [Description("Points adjusted by the system automatically")]
    System = 902,
}