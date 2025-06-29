using MediatR;
using RateRelay.Application.DTOs.Business.UserBusiness.Queries;
using RateRelay.Domain.Common;
using RateRelay.Domain.Enums;

namespace RateRelay.Application.Features.Business.Queries.GetBusinessReviews;

public class GetBusinessReviewsQuery : PagedRequest, IRequest<PagedApiResponse<GetBusinessReviewsQueryOutputDto>>
{
    public long BusinessId { get; set; }
    public BusinessReviewStatus? Status { get; set; }
}