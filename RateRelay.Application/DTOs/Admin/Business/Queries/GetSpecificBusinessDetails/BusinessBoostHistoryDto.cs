namespace RateRelay.Application.DTOs.Admin.Business.Queries.GetSpecificBusinessDetails;

public class BusinessBoostHistoryDto
{
    public byte OldPriority { get; set; }
    public byte NewPriority { get; set; }
    public required string Reason { get; set; }
    public required string ChangedByName { get; set; }
    public DateTime ChangedAt { get; set; }
    public bool WasBoosted { get; set; }
}