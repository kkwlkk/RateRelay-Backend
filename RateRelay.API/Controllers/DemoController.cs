using MediatR;
using Microsoft.AspNetCore.Mvc;
using RateRelay.Application.Features.Queries.Demo;
using RateRelay.Domain.Common;

namespace RateRelay.API.Controllers;

public class DemoController(IMediator mediator) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetDemoData([FromQuery] string name, [FromQuery] int age)
    {
        var query = new DemoQuery
        {
            Name = name,
            Age = age
        };
        
        var result = await mediator.Send(query);
        
        return Ok(result);
    }
}