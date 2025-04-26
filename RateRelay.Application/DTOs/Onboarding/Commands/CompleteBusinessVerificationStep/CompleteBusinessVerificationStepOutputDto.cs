using RateRelay.Domain.Enums;

namespace RateRelay.Application.DTOs.Onboarding.Commands.CompleteBusinessVerificationStep;

public class CompleteBusinessVerificationStepOutputDto
{
    public AccountOnboardingStep NextStep { get; set; }
}