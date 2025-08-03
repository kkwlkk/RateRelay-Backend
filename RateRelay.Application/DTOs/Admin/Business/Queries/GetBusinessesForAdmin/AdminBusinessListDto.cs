namespace RateRelay.Application.DTOs.Admin.Business.Queries.GetBusinessesForAdmin;

public class AdminBusinessListDto
{
    public long Id { get; set; }
    public required string BusinessName { get; set; }
    public required string OwnerName { get; set; }
    public required string OwnerEmail { get; set; }
    public int CurrentReviews { get; set; }
    public int PendingReviews { get; set; }
    public decimal AverageRating { get; set; }
    public byte Priority { get; set; }
    public bool IsVerified { get; set; }
    public bool IsBoosted { get; set; }
    public DateTime DateCreatedUtc { get; set; }
    public bool IsEligibleForQueue { get; set; }
    public string? LastBoostReason { get; set; }
    public long? BoostTargetReviews { get; set; }
}