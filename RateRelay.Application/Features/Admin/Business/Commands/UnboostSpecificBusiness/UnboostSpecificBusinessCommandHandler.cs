using MediatR;
using RateRelay.Domain.Common.DTOs;
using RateRelay.Domain.Interfaces;
using RateRelay.Infrastructure.Services;

namespace RateRelay.Application.Features.Admin.Business.Commands.UnboostSpecificBusiness;

public class UnboostSpecificBusinessCommandHandler(
    CurrentUserContext currentUserContext,
    IBusinessBoostService businessBoostService
) : IRequestHandler<UnboostSpecificBusinessCommand, BusinessBoostResultDto>
{
    public async Task<BusinessBoostResultDto> Handle(UnboostSpecificBusinessCommand request,
        CancellationToken cancellationToken)
    {
        return await businessBoostService.UnboostBusinessAsync(request.BusinessId, currentUserContext.AccountId,
            request.Reason, cancellationToken);
    }
}