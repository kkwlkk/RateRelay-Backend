namespace RateRelay.Application.DTOs.User.Auth.Commands;

public class GoogleAuthInputDto
{
    public required string OAuthIdToken { get; set; }
    public string? ReferralCode { get; set; }
}