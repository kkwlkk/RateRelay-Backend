namespace RateRelay.Application.DTOs.Admin.Business.Commands.CreateBusiness;

public class CreateBusinessInputDto
{
    public string PlaceId { get; set; } = string.Empty;
    public long OwnerId { get; set; }
    public bool IsVerified { get; set; } = false;
}