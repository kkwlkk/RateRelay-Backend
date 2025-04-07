using MediatR;
using Microsoft.AspNetCore.Mvc;
using RateRelay.Application.Features.Queries;
using RateRelay.Application.Features.Queries.Demo;

namespace RateRelay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DemoController(IMediator mediator) : ControllerBase
{
    [HttpGet("demo")]
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