using RateRelay.Domain.Enums;

namespace RateRelay.Application.DTOs.ReviewableBusiness.Commands;

public class SubmitBusinessReviewInputDto
{
    public required BusinessRating Rating { get; set; }
    public required string Comment { get; set; }
    public required bool PostedGoogleReview { get; set; }
}