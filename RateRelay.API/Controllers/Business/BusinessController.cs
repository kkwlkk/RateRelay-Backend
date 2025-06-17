using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using RateRelay.API.Attributes.Auth;
using RateRelay.Application.DTOs.Business.UserBusiness.Queries;
using RateRelay.Application.Features.Business.Queries.GetAllUserBusinesses;
using RateRelay.Application.Features.Business.Queries.GetBusiness;
using RateRelay.Application.Features.Business.Queries.GetBusinessReviews;

namespace RateRelay.API.Controllers.Business;

[ApiController]
[Area("Account")]
[Route("api/business")]
[RequireVerifiedBusiness]
public class BusinessController(IMapper mapper, IMediator mediator) : BaseController
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
    public async Task<IActionResult> GetReviewsByBusinessId(long businessId, [FromQuery] GetBusinessReviewsQueryInputDto input)
    {
        var query = mapper.Map<GetBusinessReviewsQuery>(input);
        query.BusinessId = businessId;
        var response = await mediator.Send(query);
        return PagedSuccess(response);
    }
}