namespace RateRelay.Application.DTOs.Admin.Business.Commands.BoostBusiness;

public class BoostBusinessInputDto
{
    public required string Reason { get; set; }
    public int? TargetReviews { get; set; } = 15;
}