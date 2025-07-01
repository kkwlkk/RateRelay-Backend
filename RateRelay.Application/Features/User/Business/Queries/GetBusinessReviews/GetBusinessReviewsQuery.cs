using MediatR;
using RateRelay.Application.DTOs.User.Business.UserBusiness.Queries;
using RateRelay.Domain.Common;
using RateRelay.Domain.Enums;

namespace RateRelay.Application.Features.User.Business.Queries.GetBusinessReviews;

public class GetBusinessReviewsQuery : PagedRequest, IRequest<PagedApiResponse<GetBusinessReviewsQueryOutputDto>>
{
    public long? ReviewId { get; set; }
    public long BusinessId { get; set; }
    public BusinessReviewStatus? Status { get; set; }
}