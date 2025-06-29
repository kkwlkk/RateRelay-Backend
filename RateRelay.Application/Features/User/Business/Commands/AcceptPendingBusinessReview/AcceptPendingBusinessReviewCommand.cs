using MediatR;
using RateRelay.Application.DTOs.Business.BusinessReviews.Commands;

namespace RateRelay.Application.Features.Business.Commands.AcceptPendingBusinessReview;

public class AcceptPendingBusinessReviewCommand : IRequest<AcceptPendingBusinessReviewOutputDto>
{
    public long ReviewId { get; set; }
}