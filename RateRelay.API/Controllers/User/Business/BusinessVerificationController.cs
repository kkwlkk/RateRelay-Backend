using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RateRelay.API.Attributes.RateLimiting;
using RateRelay.Application.DTOs.Business.BusinessVerification.Commands;
using RateRelay.Application.DTOs.Business.BusinessVerification.Queries;
using RateRelay.Application.Features.Business.Commands.InitiateBusinessVerification;
using RateRelay.Application.Features.Business.Commands.ProcessBusinessVerificationChallenge;
using RateRelay.Application.Features.Business.Queries.GetBusinessVerificationChallenge;
using RateRelay.Application.Features.Business.Queries.GetBusinessVerificationStatus;

namespace RateRelay.API.Controllers.User.Business;

[ApiController]
[Area("Account")]
[Route("api/user/business/verification")]
[Authorize]
public class BusinessVerificationController(
    IMediator mediator,
    IMapper mapper
) : UserBaseController
{
    [HttpPost("initiate")]
    [ProducesResponseType(typeof(BusinessVerificationOutputDto), StatusCodes.Status200OK)]
    [RateLimit(5, 60)]
    public async Task<IActionResult> InitiateVerification([FromBody] InitiateBusinessVerificationInputDto dto)
    {
        var command = mapper.Map<InitiateBusinessVerificationCommand>(dto);
        var response = await mediator.Send(command);
        return Success(response);
    }

    [HttpGet("challenge")]
    [ProducesResponseType(typeof(BusinessVerificationChallengeOutputDto), StatusCodes.Status200OK)]
    [RateLimit(5, 60)]
    public async Task<IActionResult> GetVerificationChallenge()
    {
        var query = new GetBusinessVerificationChallengeQuery();
        var response = await mediator.Send(query);
        return Success(response);
    }

    // processes the challenge for business associated with account id.
    // Only one business can be associated with an account
    [HttpPost("process")]
    [ProducesResponseType(typeof(BusinessVerificationStatusOutputDto), StatusCodes.Status200OK)]
    [RateLimit(10, 60)]
    public async Task<IActionResult> ProcessVerificationChallenge()
    {
        var command = new ProcessBusinessVerificationChallengeCommand();
        var response = await mediator.Send(command);
        return Success(response);
    }

    [HttpGet("status")]
    [ProducesResponseType(typeof(BusinessVerificationStatusOutputDto), StatusCodes.Status200OK)]
    [RateLimit(10, 60)]
    public async Task<IActionResult> GetBusinessVerificationStatus()
    {
        var response = await mediator.Send(new GetBusinessVerificationStatusQuery());
        return Success(response);
    }
}