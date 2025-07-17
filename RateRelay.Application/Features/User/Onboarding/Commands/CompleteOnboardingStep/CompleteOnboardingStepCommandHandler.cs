using MediatR;
using RateRelay.Application.DTOs.Onboarding.Commands.CompleteOnboardingStep;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Enums;
using RateRelay.Domain.Exceptions;
using RateRelay.Domain.Interfaces;
using RateRelay.Domain.Interfaces.DataAccess;
using RateRelay.Infrastructure.Services;

namespace RateRelay.Application.Features.Onboarding.Commands.CompleteOnboardingStep;

public class CompleteOnboardingCommandHandler(
    CurrentUserContext currentUserContext,
    IOnboardingService onboardingService,
    IUnitOfWorkFactory unitOfWorkFactory
) : IRequestHandler<CompleteOnboardingStepCommand, CompleteOnboardingStepOutputDto>
{
    public async Task<CompleteOnboardingStepOutputDto> Handle(CompleteOnboardingStepCommand request,
        CancellationToken cancellationToken)
    {
        var currentStep = await onboardingService.GetCurrentStepAsync(currentUserContext.AccountId, cancellationToken);

        if (currentStep < AccountOnboardingStep.BusinessVerification)
        {
            return new CompleteOnboardingStepOutputDto
            {
                CompletedAt = DateTime.UtcNow
            };
        }

        await onboardingService.UpdateStepAsync(
            currentUserContext.AccountId,
            AccountOnboardingStep.Completed,
            cancellationToken);

        return new CompleteOnboardingStepOutputDto
        {
            CompletedAt = DateTime.UtcNow
        };
    }
}