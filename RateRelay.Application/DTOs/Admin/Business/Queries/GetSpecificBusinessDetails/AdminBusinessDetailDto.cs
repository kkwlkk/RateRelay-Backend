namespace RateRelay.Application.DTOs.Admin.Business.Queries.GetSpecificBusinessDetails;

public class AdminBusinessDetailDto
{
    public long Id { get; set; }
    public required string BusinessName { get; set; }
    public required string PlaceId { get; set; }
    public required string Cid { get; set; }
    public required string MapUrl { get; set; }
    public long OwnerAccountId { get; set; }
    public required string OwnerName { get; set; }
    public required string OwnerEmail { get; set; }
    public int OwnerPointBalance { get; set; }
    public byte Priority { get; set; }
    public bool IsVerified { get; set; }
    public bool IsBoosted { get; set; }
    public DateTime DateCreatedUtc { get; set; }
    public int TotalReviews { get; set; }
    public int AcceptedReviews { get; set; }
    public int PendingReviews { get; set; }
    public int RejectedReviews { get; set; }
    public decimal AverageRating { get; set; }
    public DateTime? LastReviewDate { get; set; }
    public bool IsEligibleForQueue { get; set; }
    public List<BusinessBoostHistoryDto> BoostHistory { get; set; } = new();
    public long? BoostTargetReviews { get; set; } = null;
}