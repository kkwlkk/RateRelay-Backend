using RateRelay.Domain.Common;
using RateRelay.Domain.Enums;

namespace RateRelay.Application.DTOs.User.Business.UserBusiness.Queries;

public class GetBusinessReviewsQueryInputDto : PagedRequest
{
    public BusinessReviewStatus? Status { get; set; }
}