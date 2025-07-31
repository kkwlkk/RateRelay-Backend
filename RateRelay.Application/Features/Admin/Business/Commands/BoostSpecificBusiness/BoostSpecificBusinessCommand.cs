using MediatR;
using RateRelay.Domain.Common.DTOs;

namespace RateRelay.Application.Features.Admin.Business.Commands.BoostSpecificBusiness;

public class BoostSpecificBusinessCommand : IRequest<BusinessBoostResultDto>
{
    public long BusinessId { get; set; }
    public required string Reason { get; set; }
    public int TargetReviews { get; set; } = 15;
}