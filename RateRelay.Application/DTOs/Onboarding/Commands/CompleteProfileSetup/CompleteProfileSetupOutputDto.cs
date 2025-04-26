using RateRelay.Domain.Enums;

namespace RateRelay.Application.DTOs.Onboarding.Commands.CompleteProfileSetup;

public class CompleteProfileSetupOutputDto
{
    public AccountOnboardingStep NextStep { get; set; }
}