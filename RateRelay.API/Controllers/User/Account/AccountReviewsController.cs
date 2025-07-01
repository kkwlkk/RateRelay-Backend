using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RateRelay.API.Attributes.Auth;
using RateRelay.Application.DTOs.Account.ReviewHistory.Queries;
using RateRelay.Application.Features.Account.Queries.ReviewHistory;
using RateRelay.Domain.Common;

namespace RateRelay.API.Controllers.User.Account;

[ApiController]
[Area("Account")]
[Route("api/user/account/reviews")]
[Authorize]
[RequireVerifiedBusiness]
public class AccountReviewsController(IMapper mapper, IMediator mediator) : UserBaseController
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedApiResponse<AccountReviewHistoryQueryOutputDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAccountReviewHistory([FromQuery] AccountReviewHistoryQueryInputDto input)
    {
        var query = mapper.Map<GetAccountReviewHistoryQuery>(input);
        var response = await mediator.Send(query);
        return PagedSuccess(response);
    }
}