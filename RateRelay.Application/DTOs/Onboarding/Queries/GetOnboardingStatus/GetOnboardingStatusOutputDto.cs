using RateRelay.Domain.Enums;

namespace RateRelay.Application.DTOs.Onboarding.Queries.GetOnboardingStatus;

public class GetOnboardingStatusOutputDto
{
    public AccountOnboardingStep CurrentStep { get; set; }
    public string CurrentStepName { get; set; } = string.Empty;
    public DateTime? LastUpdated { get; set; }
    public bool IsCompleted { get; set; }
    public List<string> CompletedSteps { get; set; } = [];
    public List<string> RemainingSteps { get; set; } = [];
}