using MediatR;
using RateRelay.Application.DTOs.Onboarding.Commands.CompleteWelcomeStep;
using RateRelay.Domain.Enums;
using RateRelay.Domain.Interfaces;
using RateRelay.Infrastructure.Services;

namespace RateRelay.Application.Features.Onboarding.Commands.CompleteWelcomeStep;

public class WelcomeStepCommandHandler(
    CurrentUserContext currentUserContext,
    IOnboardingService onboardingService
) : IRequestHandler<CompleteWelcomeStepCommand, CompleteWelcomeStepOutputDto>
{
    public async Task<CompleteWelcomeStepOutputDto> Handle(CompleteWelcomeStepCommand request,
        CancellationToken cancellationToken)
    {
        await onboardingService.UpdateStepAsync(
            currentUserContext.AccountId,
            AccountOnboardingStep.ProfileSetup,
            cancellationToken);

        return new CompleteWelcomeStepOutputDto
        {
            NextStep = AccountOnboardingStep.ProfileSetup
        };
    }
}