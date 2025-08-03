using MediatR;
using RateRelay.Application.DTOs.Admin.Business.Queries.GetSpecificBusinessDetails;

namespace RateRelay.Application.Features.Admin.Business.Queries.GetSpecificBusinessDetails;

public class GetSpecificBusinessDetailsQuery : IRequest<AdminBusinessDetailDto>
{
    public long BusinessId { get; set; }
}