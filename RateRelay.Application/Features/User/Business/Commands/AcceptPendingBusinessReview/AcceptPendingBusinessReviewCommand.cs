using MediatR;
using RateRelay.Application.DTOs.Business.BusinessReviews.Commands;

namespace RateRelay.Application.Features.User.Business.Commands.AcceptPendingBusinessReview;

public class AcceptPendingBusinessReviewCommand : IRequest<AcceptPendingBusinessReviewOutputDto>
{
    public long BusinessId { get; set; }
    public long ReviewId { get; set; }
}