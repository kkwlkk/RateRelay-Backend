using AutoMapper;
using MediatR;
using RateRelay.Application.DTOs.Business.BusinessVerification.Queries;
using RateRelay.Application.Exceptions;
using RateRelay.Domain.Exceptions;
using RateRelay.Domain.Interfaces;
using RateRelay.Domain.Interfaces.DataAccess;
using RateRelay.Infrastructure.Services;

namespace RateRelay.Application.Features.Business.Queries.GetBusinessVerificationChallenge;

public class GetBusinessVerificationChallengeQueryHandler(
    CurrentUserContext currentUserContext,
    IUnitOfWorkFactory unitOfWorkFactory,
    IBusinessVerificationService businessVerificationService,
    IMapper mapper
) : IRequestHandler<GetBusinessVerificationChallengeQuery, BusinessVerificationChallengeOutputDto>
{
    public async Task<BusinessVerificationChallengeOutputDto> Handle(GetBusinessVerificationChallengeQuery request,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();

        var verificationChallenge = await businessVerificationService.GetActiveVerificationChallengeAsync(
            currentUserContext.AccountId
        );
        
        if (verificationChallenge is null)
            throw new NotFoundException("Verification challenge not found");
        
        var businessVerificationChallengeOutputDto = mapper.Map<BusinessVerificationChallengeOutputDto>(verificationChallenge);
        return businessVerificationChallengeOutputDto;
    }
}