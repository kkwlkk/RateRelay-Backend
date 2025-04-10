using MediatR;
using RateRelay.Application.DTOs.Auth.Commands;

namespace RateRelay.Application.Features.Auth.Commands.Login;

public class LoginCommand : IRequest<LoginOutputDto>
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}