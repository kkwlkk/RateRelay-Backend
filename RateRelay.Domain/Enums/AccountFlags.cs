namespace RateRelay.Domain.Enums;

[Flags]
public enum AccountFlags
{
    /// <summary>
    /// Indicates that the user has seen the last onboarding step.
    /// </summary>
    HasSeenLastOnboardingStep = 1 << 0,
}