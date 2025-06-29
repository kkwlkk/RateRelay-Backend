using MediatR;
using Microsoft.EntityFrameworkCore;
using RateRelay.Application.DTOs.Onboarding.Commands.CompleteBusinessVerificationStep;
using RateRelay.Domain.Common;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Enums;
using RateRelay.Domain.Exceptions;
using RateRelay.Domain.Interfaces;
using RateRelay.Domain.Interfaces.DataAccess;
using RateRelay.Infrastructure.Services;

namespace RateRelay.Application.Features.Onboarding.Commands.CompleteBusinessVerificationStep;

public class CompleteBusinessVerificationStepCommandHandler(
    CurrentUserContext currentUserContext,
    IUnitOfWorkFactory unitOfWorkFactory,
    IBusinessVerificationService businessVerificationService,
    IOnboardingService onboardingService
) : IRequestHandler<CompleteBusinessVerificationStepCommand, CompleteBusinessVerificationStepOutputDto>
{
    public async Task<CompleteBusinessVerificationStepOutputDto> Handle(
        CompleteBusinessVerificationStepCommand request,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var businessRepository = unitOfWork.GetRepository<BusinessEntity>();

        var existingBusiness = await businessRepository.GetBaseQueryable()
            .FirstOrDefaultAsync(b => b.OwnerAccountId == currentUserContext.AccountId, cancellationToken);

        BusinessVerificationResult? verificationResult = null;

        if (existingBusiness is { IsVerified: true })
        {
            await onboardingService.UpdateStepAsync(
                currentUserContext.AccountId,
                AccountOnboardingStep.Welcome,
                cancellationToken);

            return new CompleteBusinessVerificationStepOutputDto
            {
                NextStep = AccountOnboardingStep.Welcome,
            };
        }

        if (existingBusiness is { IsVerified: false })
        {
            throw new AppOkException(
                "Business verification is already in progress. Please wait for the verification to complete.");
        }

        verificationResult = await businessVerificationService.InitiateVerificationAsync(
            request.PlaceId,
            currentUserContext.AccountId);

        if (verificationResult is { IsVerified: false, IsSuccess: true })
        {
            throw new AppOkException(
                "Business verification was started successfully. Please wait for the verification to complete.");
        }

        await onboardingService.UpdateStepAsync(
            currentUserContext.AccountId,
            AccountOnboardingStep.Welcome,
            cancellationToken);

        return new CompleteBusinessVerificationStepOutputDto
        {
            NextStep = AccountOnboardingStep.Welcome,
        };
    }
}