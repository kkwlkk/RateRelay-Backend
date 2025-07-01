using RateRelay.Domain.Enums;

namespace RateRelay.Application.DTOs.User.Business.UserBusiness.Queries;

public class GetBusinessReviewsQueryOutputDto
{
    public required long Id { get; set; }
    public required BusinessReviewStatus Status { get; set; }
    public required BusinessRating Rating { get; set; }
    public required string Comment { get; set; }
    public required bool PostedGoogleMapsReview { get; set; }
    public string? GoogleMapsReviewUrl { get; set; }
    public required DateTime DateCreatedUtc { get; set; }
}