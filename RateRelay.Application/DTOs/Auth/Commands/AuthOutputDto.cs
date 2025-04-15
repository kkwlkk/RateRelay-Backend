namespace RateRelay.Application.DTOs.Auth.Commands;

public class AuthOutputDto
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
    public bool IsNewUser { get; set; } = false;
}