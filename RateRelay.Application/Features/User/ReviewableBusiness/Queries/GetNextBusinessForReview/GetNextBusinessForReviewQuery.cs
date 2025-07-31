using MediatR;
using RateRelay.Application.DTOs.ReviewableBusiness.Queries;

namespace RateRelay.Application.Features.User.ReviewableBusiness.Queries.GetNextBusinessForReview;

public class GetNextBusinessForReviewQuery : IRequest<GetNextBusinessForReviewOutputDto>
{
    public bool SkipBusinessAssignment { get; set; } = false;
}