using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RateRelay.Application.DTOs.User.Account.Queries;
using RateRelay.Application.Features.Account.Queries.GetAccount;
using RateRelay.Application.Features.User.Account.Commands.UpdateAccountSettings;
using RateRelay.Application.Features.User.Account.Queries.GetAccountStatistics;

namespace RateRelay.API.Controllers.User.Account;

[ApiController]
[Area("Account")]
[Authorize]
public class AccountController(IMediator mediator) : UserBaseController
{
    [HttpGet]
    [ProducesResponseType(typeof(AccountQueryOutputDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAccount()
    {
        var query = new GetAccountQuery();
        var response = await mediator.Send(query);
        return Success(response);
    }

    [HttpPatch("settings")]
    public async Task<IActionResult> UpdateAccountSettings([FromBody] UpdateAccountSettingsCommand command)
    {
        var response = await mediator.Send(command);
        return Success(response);
    }
    
    [HttpGet("stats")]
    [ProducesResponseType(typeof(AccountStatisticsQueryOutputDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAccountStatistics()
    {
        var query = new GetAccountStatisticsQuery();
        var response = await mediator.Send(query);
        return Success(response);
    }
}