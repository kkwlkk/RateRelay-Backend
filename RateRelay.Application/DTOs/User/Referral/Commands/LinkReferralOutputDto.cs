namespace RateRelay.Application.DTOs.User.Referral.Commands;

public class LinkReferralOutputDto
{
    public string ReferrerName { get; set; } = string.Empty;
    public int WelcomeBonus { get; set; }
}