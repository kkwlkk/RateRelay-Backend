using RateRelay.Domain.Enums;

namespace RateRelay.Application.DTOs.User.Account.Queries;

public class AccountQueryOutputDto
{
    public long Id { get; set; }
    public string GoogleUsername { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public ulong Permissions { get; set; }
    public int PointBalance { get; set; }
    public RoleEntityOutputDto? Role { get; set; }
    public bool HasCompletedOnboarding { get; set; }
    public AccountOnboardingStep OnboardingStep { get; set; }
    public AccountFlags Flags { get; set; }
}