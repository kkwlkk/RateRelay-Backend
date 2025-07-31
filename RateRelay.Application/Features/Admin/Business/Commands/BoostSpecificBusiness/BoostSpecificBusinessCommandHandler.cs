using MediatR;
using RateRelay.Domain.Common.DTOs;
using RateRelay.Domain.Interfaces;
using RateRelay.Infrastructure.Services;

namespace RateRelay.Application.Features.Admin.Business.Commands.BoostSpecificBusiness;

public class BoostSpecificBusinessCommandHandler(
    CurrentUserContext currentUserContext,
    IBusinessBoostService businessBoostService
) : IRequestHandler<BoostSpecificBusinessCommand, BusinessBoostResultDto>
{
    public async Task<BusinessBoostResultDto> Handle(BoostSpecificBusinessCommand request,
        CancellationToken cancellationToken)
    {
        return await businessBoostService.BoostBusinessAsync(
            request.BusinessId,
            currentUserContext.AccountId,
            byte.MaxValue,
            request.TargetReviews,
            request.Reason,
            cancellationToken);
    }
}