namespace RateRelay.Application.DTOs.Auth.Commands;

public class RefreshTokenOutputDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}