using AutoMapper;
using MediatR;
using RateRelay.Application.DTOs.Business.BusinessVerification.Commands;
using RateRelay.Application.Exceptions;
using RateRelay.Domain.Interfaces;
using RateRelay.Domain.Interfaces.Services;
using RateRelay.Infrastructure.Services;

namespace RateRelay.Application.Features.Business.Commands.ProcessBusinessVerificationChallenge;

public class ProcessBusinessVerificationChallengeCommandHandler(
    CurrentUserContext currentUserContext,
    IBusinessVerificationService businessVerificationService,
    IMapper mapper
) : IRequestHandler<ProcessBusinessVerificationChallengeCommand, BusinessVerificationOutputDto>
{
    public async Task<BusinessVerificationOutputDto> Handle(ProcessBusinessVerificationChallengeCommand request,
        CancellationToken cancellationToken)
    {
        var verificationResult = await businessVerificationService.CheckVerificationStatusAsync(
            currentUserContext.AccountId
        );

        if (!verificationResult.IsSuccess)
        {
            throw new AppException(verificationResult.ErrorMessage);
        }

        if (verificationResult.Verification is null)
        {
            throw new NotFoundException("Verification not found.");
        }

        var businessVerificationOutputDto = mapper.Map<BusinessVerificationOutputDto>(verificationResult.Verification);
        return businessVerificationOutputDto;
    }
}