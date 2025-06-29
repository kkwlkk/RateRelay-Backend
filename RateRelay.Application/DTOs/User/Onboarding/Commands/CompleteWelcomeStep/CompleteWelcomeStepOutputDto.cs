using RateRelay.Domain.Enums;

namespace RateRelay.Application.DTOs.Onboarding.Commands.CompleteWelcomeStep;

public class CompleteWelcomeStepOutputDto
{
    public AccountOnboardingStep NextStep { get; set; }
}