using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RateRelay.Application.DTOs.Account.Queries;
using RateRelay.Application.Features.Account.Queries.GetAccount;

namespace RateRelay.API.Controllers.Account;

[ApiController]
[Area("Account")]
[Route("api/[controller]")]
[Authorize]
public class AccountController(IMediator mediator, IMapper mapper) : BaseController
{
    [HttpGet]
    [ProducesResponseType(typeof(AccountQueryOutputDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAccount()
    {
        var query = new GetAccountQuery();
        var response = await mediator.Send(query);
        return Success(response);
    }
}