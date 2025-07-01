namespace RateRelay.Application.DTOs.User.Business.UserBusiness.Queries;

public class GetBusinessQueryOutputDto
{
    public required long Id { get; set; }
    public required string PlaceId { get; set; }
    public required string Cid { get; set; }
    public required string BusinessName { get; set; }
    public required bool IsVerified { get; set; }
    public required bool IsEligibleForQueue { get; set; }
    public required DateTime DateCreatedUtc { get; set; }
    public required decimal AverageRating { get; set; }
    public required GetBusinessQueryOutputReviewsDto Reviews { get; set; }
}

public class GetBusinessQueryOutputReviewsDto
{
    public required int TotalCount { get; set; }
    public required int AcceptedCount { get; set; }
    public required int PendingCount { get; set; }
    public required int RejectedCount { get; set; }
}