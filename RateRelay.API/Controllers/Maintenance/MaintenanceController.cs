using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using RateRelay.Application.DTOs.Shared;
using RateRelay.Application.Features.Shared.Maintenance.Queries.GetMaintenanceMode;

namespace RateRelay.API.Controllers.Maintenance;

[ApiController]
[Route("api/[controller]")]
[Area("Maintenance")]
public class MaintenanceController(IMediator mediator, IMapper mapper) : BaseController
{
    [HttpGet]
    [ProducesResponseType(typeof(GetMaintenanceModeOutputDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMaintenanceMode()
    {
        var query = new GetMaintenanceModeQuery();
        var response = await mediator.Send(query);
        var outputDto = mapper.Map<GetMaintenanceModeOutputDto>(response);
        return Success(outputDto);
    }
}