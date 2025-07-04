using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RateRelay.Application.DTOs.Auth.Commands;
using RateRelay.Application.DTOs.User.Auth.Commands;
using RateRelay.Application.Features.Auth.Commands.Google;
using RateRelay.Application.Features.Auth.Commands.RefreshToken;
using RateRelay.Application.Features.User.Auth.Commands.Google;

namespace RateRelay.API.Controllers.Auth;

[ApiController]
[Area("Auth")]
[Route("api/[controller]")]
public class AuthController(IMediator mediator, IMapper mapper) : BaseController
{
    [HttpPost("google")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthOutputDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] GoogleAuthInputDto googleAuthInputDto)
    {
        var command = mapper.Map<GoogleAuthCommand>(googleAuthInputDto);
        var response = await mediator.Send(command);
        return Success(response);
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthOutputDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenInputDto refreshTokenInputDto)
    {
        var command = mapper.Map<RefreshTokenCommand>(refreshTokenInputDto);
        var response = await mediator.Send(command);
        return Success(response);
    }
}