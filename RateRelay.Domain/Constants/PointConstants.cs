using System.ComponentModel;

namespace RateRelay.Domain.Constants;

public static class PointConstants
{
    [Description("Standard point amount for basic review transactions")]
    public const int BasicReviewPoints = 1;
    
    [Description("Additional point amount for optional Google Maps review transactions")]
    public const int GoogleMapsReviewPoints = 1;
    
    [Description("Minimum points required for a business owner to receive reviews and maintain platform visibility")]
    public const int MinimumOwnerPointBalanceForBusinessVisibility = 1;
}