using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using RateRelay.API.Attributes.Auth;
using RateRelay.API.Attributes.RateLimiting;
using RateRelay.Application.DTOs.ReviewableBusiness.Commands;
using RateRelay.Application.DTOs.ReviewableBusiness.Queries;
using RateRelay.Application.Features.ReviewableBusiness.Commands.SubmitBusinessReview;
using RateRelay.Application.Features.ReviewableBusiness.Queries.GetTimeLeftForBusinessReview;
using RateRelay.Application.Features.User.ReviewableBusiness.Queries.GetNextBusinessForReview;

namespace RateRelay.API.Controllers.User.ReviewableBusiness;

[ApiController]
[Area("Account")]
[Route("api/user/reviewable-businesses")]
[RequireVerifiedBusiness]
public class ReviewableBusinessQueueController(
    IMediator mediator,
    IMapper mapper
) : UserBaseController
{
    [HttpGet("next")]
    [ProducesResponseType(typeof(GetNextBusinessForReviewOutputDto), StatusCodes.Status200OK)]
    [RateLimit(50, 60)]
    public async Task<IActionResult> GetNextReviewableBusinessAsync([FromQuery] GetNextBusinessForReviewInputDto input)
    {
        var query = mapper.Map<GetNextBusinessForReviewQuery>(input);
        var response = await mediator.Send(query);
        return Success(response);
    }

    [HttpGet("time-left")]
    [ProducesResponseType(typeof(GetTimeLeftForBusinessReviewOutputDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTimeLeftForBusinessReviewAsync()
    {
        var query = new GetTimeLeftForBusinessReviewQuery();
        var response = await mediator.Send(query);
        return Success(response);
    }

    [HttpPost("submit")]
    [ProducesResponseType(typeof(SubmitBusinessReviewOutputDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> SubmitBusinessReviewAsync([FromBody] SubmitBusinessReviewInputDto input)
    {
        var command = mapper.Map<SubmitBusinessReviewCommand>(input);
        var response = await mediator.Send(command);
        return Success(response);
    }
}