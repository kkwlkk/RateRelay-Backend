using MediatR;
using RateRelay.Application.DTOs.Business.BusinessReviews.Commands;

namespace RateRelay.Application.Features.Business.Commands.RejectPendingBusinessReview;

public class RejectPendingBusinessReviewCommand : IRequest<RejectPendingBusinessReviewOutputDto>
{
    public long ReviewId { get; set; }
}