using MediatR;
using RateRelay.Application.DTOs.ReviewableBusiness.Commands;
using RateRelay.Domain.Enums;

namespace RateRelay.Application.Features.ReviewableBusiness.Commands.SubmitBusinessReview;

public class SubmitBusinessReviewCommand : IRequest<SubmitBusinessReviewOutputDto>
{
    public required BusinessRating Rating { get; set; }
    public required string Comment { get; set; }
    public required bool PostedGoogleReview { get; set; }
}