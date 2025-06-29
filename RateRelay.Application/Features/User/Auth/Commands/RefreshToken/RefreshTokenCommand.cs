using MediatR;
using RateRelay.Application.DTOs.Auth.Commands;

namespace RateRelay.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommand : IRequest<RefreshTokenOutputDto>
{
    public required string RefreshToken { get; set; }
}