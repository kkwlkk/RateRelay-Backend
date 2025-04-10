using System.ComponentModel.DataAnnotations;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RateRelay.Application.DTOs.Auth.Commands;
using RateRelay.Application.Features.Auth.Commands.Login;
using RateRelay.Application.Features.Auth.Commands.RefreshToken;

namespace RateRelay.API.Controllers.Auth;

[ApiController]
[Area("Auth")]
[Route("api/[controller]")]
public class AuthController(IMediator mediator, IMapper mapper) : BaseController
{
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginOutputDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] [Required] LoginInputDto loginInputDto)
    {
        var command = mapper.Map<LoginCommand>(loginInputDto);
        var response = await mediator.Send(command);
        return Ok(response);
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginOutputDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenInputDto refreshTokenInputDto)
    {
        var command = mapper.Map<RefreshTokenCommand>(refreshTokenInputDto);
        var response = await mediator.Send(command);
        return Ok(response);
    }
}