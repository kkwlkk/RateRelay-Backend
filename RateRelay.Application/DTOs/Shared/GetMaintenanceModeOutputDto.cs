namespace RateRelay.Application.DTOs.Shared;

public class GetMaintenanceModeOutputDto
{
    public bool IsActive { get; set; }
    public string? Reason { get; set; }
}