using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RateRelay.API.Attributes.Auth;
using RateRelay.Application.DTOs.Account.ReviewHistory.Queries;
using RateRelay.Application.Features.Account.Queries.ReviewHistory;
using RateRelay.Domain.Common;

namespace RateRelay.API.Controllers.Account;

[ApiController]
[Area("Account")]
[Route("api/account")]
[Authorize]
[RequireVerifiedBusiness]
public class AccountReviewHistoryController(IMapper mapper, IMediator mediator) : BaseController
{
    [HttpGet("review-history")]
    [ProducesResponseType(typeof(PagedApiResponse<AccountReviewHistoryQueryOutputDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAccountReviewHistory([FromQuery] AccountReviewHistoryQueryInputDto input)
    {
        var query = mapper.Map<GetAccountReviewHistoryQuery>(input);
        var response = await mediator.Send(query);
        return PagedSuccess(response);
    }
}