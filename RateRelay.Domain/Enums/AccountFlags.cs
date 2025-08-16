namespace RateRelay.Domain.Enums;

[Flags]
public enum AccountFlags
{
    None = 0,

    /// <summary>
    /// Indicates that the user has seen the last onboarding step.
    /// </summary>
    HasSeenLastOnboardingStep = 1 << 0,
    
    /// <summary>
    /// Bypasses the verified business requirement for certain operations.
    /// </summary>
    BypassVerifiedBusinessRequirement = 1 << 1,
}