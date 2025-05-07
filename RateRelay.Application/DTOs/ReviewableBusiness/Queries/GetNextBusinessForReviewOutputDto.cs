namespace RateRelay.Application.DTOs.ReviewableBusiness.Queries;

public class GetNextBusinessForReviewOutputDto
{
    public required long BusinessId { get; set; }

    public required string PlaceId { get; set; }

    public required string Cid { get; set; }

    public required string BusinessName { get; set; }
    public required string MapUrl { get; set; }
    public required int InitialReviewTimeInSeconds { get; set; }
    public required int RemainingReviewTimeInSeconds { get; set; }
}