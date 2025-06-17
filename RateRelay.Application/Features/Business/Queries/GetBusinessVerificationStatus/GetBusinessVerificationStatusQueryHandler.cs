using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RateRelay.Application.DTOs.Business.BusinessVerification.Commands;
using RateRelay.Application.Exceptions;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Exceptions;
using RateRelay.Domain.Interfaces;
using RateRelay.Domain.Interfaces.Services;
using RateRelay.Infrastructure.Services;

namespace RateRelay.Application.Features.Business.Queries.GetBusinessVerificationStatus;

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
            throw new NotFoundException("No active verification challenge found.");
        }

        var verificationStatus = mapper.Map<BusinessVerificationStatusOutputDto>(activeVerificationChallenge);
        return verificationStatus;
    }
}