using MediatR;
using RateRelay.Domain.Common.DTOs;

namespace RateRelay.Application.Features.Admin.Business.Commands.UnboostSpecificBusiness;

public class UnboostSpecificBusinessCommand : IRequest<BusinessBoostResultDto>
{
    public long BusinessId { get; set; }
    public required string Reason { get; set; }
}