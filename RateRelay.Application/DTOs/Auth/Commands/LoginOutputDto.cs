namespace RateRelay.Application.DTOs.Auth.Commands;

public class LoginOutputDto
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
}