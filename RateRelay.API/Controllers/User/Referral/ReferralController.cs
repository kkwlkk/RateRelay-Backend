using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RateRelay.API.Attributes.Auth;
using RateRelay.Application.DTOs.User.Referral.Commands;
using RateRelay.Application.DTOs.User.Referral.Queries;
using RateRelay.Application.Features.User.Referral.Commands.GenerateReferralCode;
using RateRelay.Application.Features.User.Referral.Commands.LinkReferral;
using RateRelay.Application.Features.User.Referral.Queries.ReferralGoals;
using RateRelay.Application.Features.User.Referral.Queries.ReferralStats;

namespace RateRelay.API.Controllers.User.Referral;

[ApiController]
[Area("Referral")]
[Route("api/user/referral")]
[Authorize]
[RequireOnboardingStep]
public class ReferralController(IMediator mediator, IMapper mapper) : UserBaseController
{
    [HttpGet("stats")]
    [ProducesResponseType(typeof(ReferralStatsOutputDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReferralStats(CancellationToken cancellationToken)
    {
        var query = new GetReferralStatsQuery();
        var response = await mediator.Send(query, cancellationToken);
        return Success(response);
    }
    
    [HttpGet("goals")]
    [ProducesResponseType(typeof(List<ReferralGoalOutputDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReferralGoals(CancellationToken cancellationToken)
    {
        var query = new GetReferralGoalsQuery();
        var response = await mediator.Send(query, cancellationToken);
        return Success(response);
    }
    
    [HttpPost("generate-code")]
    [ProducesResponseType(typeof(GenerateReferralCodeOutputDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GenerateReferralCode(CancellationToken cancellationToken)
    {
        var command = new GenerateReferralCodeCommand();
        var response = await mediator.Send(command, cancellationToken);
        return Success(response);
    }
    
    [HttpPost("link")]
    [ProducesResponseType(typeof(LinkReferralOutputDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> LinkReferral([FromBody] LinkReferralInputDto input, CancellationToken cancellationToken)
    {
        var command = mapper.Map<LinkReferralCommand>(input);
        var response = await mediator.Send(command, cancellationToken);
        return Success(response);
    }
}