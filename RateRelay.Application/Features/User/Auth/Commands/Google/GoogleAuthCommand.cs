using MediatR;
using RateRelay.Application.DTOs.Auth.Commands;

namespace RateRelay.Application.Features.User.Auth.Commands.Google;

public class GoogleAuthCommand : IRequest<AuthOutputDto>
{
    public required string OAuthIdToken { get; set; }
    public string? ReferralCode { get; set; }
}