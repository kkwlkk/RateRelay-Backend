namespace RateRelay.Application.DTOs.Business.BusinessReviews.Queries;

public class GetAwaitingBusinessReviewsOutputDto
{
    public long ReviewId { get; set; }
    public DateTime SubmittedAt { get; set; }
}