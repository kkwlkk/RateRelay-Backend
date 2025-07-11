using AutoMapper;
using MediatR;
using RateRelay.Application.DTOs.Business.BusinessVerification.Commands;
using RateRelay.Application.Features.Business.Queries.GetBusinessVerificationStatus;
using RateRelay.Domain.Exceptions;
using RateRelay.Domain.Interfaces;
using RateRelay.Infrastructure.Services;

namespace RateRelay.Application.Features.User.Business.Queries.GetBusinessVerificationStatus;

public class GetBusinessVerificationStatusQueryHandler(
    CurrentUserContext currentUserContext,
    IBusinessVerificationService businessVerificationService,
    IMapper mapper
) : IRequestHandler<GetBusinessVerificationStatusQuery, BusinessVerificationStatusOutputDto>
{
    public async Task<BusinessVerificationStatusOutputDto> Handle(GetBusinessVerificationStatusQuery request,
        CancellationToken cancellationToken)
    {
        var activeVerificationChallenge = await businessVerificationService.GetActiveVerificationChallengeAsync(
            currentUserContext.AccountId
        );
        
        if (activeVerificationChallenge is null)
        {
            throw new NotFoundException("No active verification challenge found.", "NoActiveVerificationChallenge");
        }

        var verificationStatus = mapper.Map<BusinessVerificationStatusOutputDto>(activeVerificationChallenge);
        return verificationStatus;
    }
}