using MediatR;
using Microsoft.AspNetCore.Mvc;
using RateRelay.Application.Features.Accounts.Commands;
using RateRelay.Application.Features.Accounts.Queries;
using Swashbuckle.AspNetCore.Annotations;

namespace RateRelay.API.Controllers;

[Area("Accounts")]
public class AccountsController(IMediator mediator) : BaseController
{
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get account by ID",
        Description = "Retrieves account information by account ID."
    )]
    public async Task<IActionResult> GetAccount([FromQuery] long accountId)
    {
        var query = new AccountsQuery
        {
            AccountId = accountId
        };

        var result = await mediator.Send(query);

        return Ok(result);
    }
    
    [HttpPost]
    [SwaggerOperation(
        Summary = "Create a new account",
        Description = "Creates a new account with the provided information."
    )]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountCommand command)
    {
        var result = await mediator.Send(command);

        return Ok(result);
    }
}