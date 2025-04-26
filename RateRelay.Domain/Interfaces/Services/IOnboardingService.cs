using RateRelay.Domain.Enums;

namespace RateRelay.Domain.Interfaces;

public interface IOnboardingService
{
    Task<AccountOnboardingStep> GetCurrentStepAsync(long accountId, CancellationToken cancellationToken = default);
    Task<bool> UpdateStepAsync(long accountId, AccountOnboardingStep step, CancellationToken cancellationToken = default);
    Task<bool> ValidateStepTransitionAsync(long accountId, AccountOnboardingStep step, AccountOnboardingStep nextStep, CancellationToken cancellationToken = default);
    Task<bool> HasCompletedOnboardingAsync(long accountId, CancellationToken cancellationToken = default);
    Task<bool> IsStepAccessibleAsync(long accountId, AccountOnboardingStep step, CancellationToken cancellationToken = default);
}