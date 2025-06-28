using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RateRelay.API.Attributes.Auth;
using RateRelay.API.Attributes.RateLimiting;
using RateRelay.Application.DTOs.Business.BusinessReviews.Commands;
using RateRelay.Application.DTOs.Business.BusinessReviews.Queries;
using RateRelay.Application.Features.Business.Commands.AcceptPendingBusinessReview;
using RateRelay.Application.Features.Business.Commands.RejectPendingBusinessReview;
using RateRelay.Application.Features.Business.Commands.ReportBusinessReview;
using RateRelay.Application.Features.Business.Queries.GetAwaitingBusinessReviews;
using RateRelay.Domain.Enums;

namespace RateRelay.API.Controllers.Business;

[ApiController]
[Area("Account")]
[Route("api/business/reviews")]
[RequireVerifiedBusiness]
public class BusinessReviewsController(
    IMediator mediator,
    IMapper mapper
) : BaseController
{
    [HttpGet("awaiting")]
    [ProducesResponseType(typeof(List<GetAwaitingBusinessReviewsOutputDto>), StatusCodes.Status200OK)]
    [RateLimit(5, 60)]
    public async Task<IActionResult> GetAwaitingBusinessReviews([FromQuery] GetAwaitingBusinessReviewsInputDto dto)
    {
        var query = mapper.Map<GetAwaitingBusinessReviewsQuery>(dto);
        var response = await mediator.Send(query);
        return Success(response);
    }

    [HttpPost("accept")]
    [ProducesResponseType(typeof(AcceptPendingBusinessReviewOutputDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> AcceptPendingBusinessReview([FromBody] AcceptPendingBusinessReviewInputDto dto)
    {
        var command = mapper.Map<AcceptPendingBusinessReviewCommand>(dto);
        var response = await mediator.Send(command);
        return Success(response);
    }
    
    // [HttpPost("reject")]
    // [ProducesResponseType(typeof(RejectPendingBusinessReviewOutputDto), StatusCodes.Status200OK)]
    // public async Task<IActionResult> RejectPendingBusinessReview([FromBody] RejectPendingBusinessReviewInputDto dto)
    // {
    //     var command = mapper.Map<RejectPendingBusinessReviewCommand>(dto);
    //     var response = await mediator.Send(command);
    //     return Success(response);
    // }
    
    [HttpPost("report")]
    [ProducesResponseType(typeof(ReportBusinessReviewOutputDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ReportBusinessReview([FromBody] ReportBusinessReviewInputDto dto)
    {
        var command = mapper.Map<ReportBusinessReviewCommand>(dto);
        var response = await mediator.Send(command);
        return Success(response);
    }
}