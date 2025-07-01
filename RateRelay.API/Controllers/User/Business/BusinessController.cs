using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using RateRelay.API.Attributes.Auth;
using RateRelay.Application.DTOs.Business.BusinessReviews.Commands;
using RateRelay.Application.DTOs.User.Business.BusinessReviews.Commands;
using RateRelay.Application.DTOs.User.Business.UserBusiness.Queries;
using RateRelay.Application.Features.Business.Commands.AcceptPendingBusinessReview;
using RateRelay.Application.Features.Business.Queries.GetBusiness;
using RateRelay.Application.Features.User.Business.Commands.AcceptPendingBusinessReview;
using RateRelay.Application.Features.User.Business.Commands.ReportBusinessReview;
using RateRelay.Application.Features.User.Business.Queries.GetAllUserBusinesses;
using RateRelay.Application.Features.User.Business.Queries.GetBusinessReviews;

namespace RateRelay.API.Controllers.User.Business;

[ApiController]
[Area("Account")]
[RequireVerifiedBusiness]
public class BusinessController(IMapper mapper, IMediator mediator) : UserBaseController
{
    [HttpGet]
    [ProducesResponseType(typeof(List<GetBusinessQueryOutputDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] GetAllUserBusinessesQueryInputDto dto)
    {
        var query = mapper.Map<GetAllUserBusinessesQuery>(dto);
        var response = await mediator.Send(query);
        return PagedSuccess(response);
    }

    [HttpGet("{businessId:int}")]
    [ProducesResponseType(typeof(GetBusinessQueryOutputDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int businessId)
    {
        var query = new GetBusinessQuery(businessId);
        var response = await mediator.Send(query);
        return Success(response);
    }

    [HttpGet("{businessId:long}/reviews")]
    [ProducesResponseType(typeof(List<GetBusinessReviewsQueryOutputDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReviewsByBusinessId(long businessId,
        [FromQuery] GetBusinessReviewsQueryInputDto input)
    {
        var query = mapper.Map<GetBusinessReviewsQuery>(input);
        query.BusinessId = businessId;
        var response = await mediator.Send(query);
        return PagedSuccess(response);
    }

    [HttpGet("{businessId:long}/reviews/{reviewId:long}")]
    [ProducesResponseType(typeof(GetBusinessReviewsQueryOutputDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReviewById(long businessId, long reviewId)
    {
        var query = new GetBusinessReviewsQuery { BusinessId = businessId, ReviewId = reviewId };
        var response = await mediator.Send(query);
        return Success(response);
    }

    [HttpPost("{businessId:long}/reviews/{reviewId:long}/accept")]
    [ProducesResponseType(typeof(AcceptPendingBusinessReviewOutputDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> AcceptPendingBusinessReview(long businessId, long reviewId)
    {
        var command = new AcceptPendingBusinessReviewCommand
        {
            BusinessId = businessId,
            ReviewId = reviewId
        };
        var response = await mediator.Send(command);
        return Success(response);
    }

    [HttpPost("{businessId:long}/reviews/{reviewId:long}/report")]
    [ProducesResponseType(typeof(ReportBusinessReviewOutputDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ReportBusinessReview(long businessId, long reviewId,
        [FromBody] ReportBusinessReviewInputDto dto)
    {
        var command = mapper.Map<ReportBusinessReviewCommand>(dto);
        command.BusinessId = businessId;
        command.ReviewId = reviewId;
        var response = await mediator.Send(command);
        return Success(response);
    }
}