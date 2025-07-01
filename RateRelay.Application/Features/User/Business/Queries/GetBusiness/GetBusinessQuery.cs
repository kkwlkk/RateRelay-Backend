using MediatR;
using RateRelay.Application.DTOs.User.Business.UserBusiness.Queries;

namespace RateRelay.Application.Features.Business.Queries.GetBusiness;

public class GetBusinessQuery(long businessId) : IRequest<GetBusinessQueryOutputDto>
{
    public long BusinessId { get; set; } = businessId;
}