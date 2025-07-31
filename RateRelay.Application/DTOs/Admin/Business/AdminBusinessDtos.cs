using RateRelay.Domain.Common;

namespace RateRelay.Application.DTOs.Admin.Business;

// TODO: cleanup and move to their own files

public class AdminBusinessFilterDto
{
    public int? MinReviews { get; set; }
    public int? MaxReviews { get; set; }
    public bool? IsVerified { get; set; }
    public long? OwnerId { get; set; }
}

public class AdminBusinessInputDto : PagedRequest<AdminBusinessFilterDto>;

public class BoostBusinessInputDto
{
    public required string Reason { get; set; }
    public int? TargetReviews { get; set; } = 15;
}

public class UnboostBusinessInputDto
{
    public string? Reason { get; set; }
}

public class BoostSelectedBusinessesInputDto
{
    public required List<long> BusinessIds { get; set; }
    public required string Reason { get; set; }
    public int? TargetReviews { get; set; } = 15;
}

public class AdminBusinessListDto
{
    public long Id { get; set; }
    public required string BusinessName { get; set; }
    public required string OwnerDisplayName { get; set; }
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

public class AdminBusinessDetailDto
{
    public long Id { get; set; }
    public required string BusinessName { get; set; }
    public required string PlaceId { get; set; }
    public required string Cid { get; set; }
    public required string MapUrl { get; set; }
    public long OwnerAccountId { get; set; }
    public required string OwnerDisplayName { get; set; }
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

public class BusinessBoostHistoryDto
{
    public byte OldPriority { get; set; }
    public byte NewPriority { get; set; }
    public required string Reason { get; set; }
    public required string ChangedByDisplayName { get; set; }
    public DateTime ChangedAt { get; set; }
    public bool WasBoosted { get; set; }
}

public class CreateBusinessOutputDto
{
    public long Id { get; set; }
}