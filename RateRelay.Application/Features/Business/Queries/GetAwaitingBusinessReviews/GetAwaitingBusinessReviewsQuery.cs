using MediatR;
using RateRelay.Application.DTOs.Business.BusinessReviews.Queries;

namespace RateRelay.Application.Features.Business.Queries.GetAwaitingBusinessReviews;

public class GetAwaitingBusinessReviewsQuery : IRequest<List<GetAwaitingBusinessReviewsOutputDto>>
{
    public long BusinessId { get; set; }
}