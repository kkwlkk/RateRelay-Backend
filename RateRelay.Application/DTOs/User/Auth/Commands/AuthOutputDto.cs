namespace RateRelay.Application.DTOs.User.Auth.Commands;

public class AuthOutputDto
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
    public bool IsNewUser { get; set; } = false;
    public bool? IsReferralLinked { get; set; } = null;
}