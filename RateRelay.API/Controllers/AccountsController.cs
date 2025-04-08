using MediatR;
using Microsoft.AspNetCore.Mvc;
using RateRelay.Application.Features.Queries.Accounts;
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
}