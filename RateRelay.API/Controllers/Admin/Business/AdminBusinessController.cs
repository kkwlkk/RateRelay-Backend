using MediatR;
using Microsoft.AspNetCore.Mvc;
using RateRelay.API.Attributes.Auth;
using RateRelay.Application.DTOs.Admin.Business;
using RateRelay.Application.DTOs.Admin.Business.Commands.CreateBusiness;
using RateRelay.Application.Features.Admin.Business.Commands.BoostSpecificBusiness;
using RateRelay.Application.Features.Admin.Business.Commands.CreateBusiness;
using RateRelay.Application.Features.Admin.Business.Commands.DeleteBusiness;
using RateRelay.Application.Features.Admin.Business.Commands.UnboostSpecificBusiness;
using RateRelay.Application.Features.Admin.Business.Queries.GetBusinessesForAdmin;
using RateRelay.Application.Features.Admin.Business.Queries.GetSpecificBusinessDetails;
using RateRelay.Domain.Common;
using RateRelay.Domain.Common.DTOs;
using RateRelay.Domain.Enums;

namespace RateRelay.API.Controllers.Admin.Business;

[ApiController]
[Area("Admin")]
[Route("api/admin/businesses")]
public class AdminBusinessController(IMediator mediator) : AdminBaseController
{
    [HttpDelete("{businessId:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [RequirePermission(Permission.DeleteBusiness)]
    public async Task<IActionResult> DeleteBusiness(long businessId)
    {
        var command = new DeleteBusinessCommand { BusinessId = businessId };
        await mediator.Send(command);
        return NoContent();
    }

    [HttpPost]
    [ProducesResponseType(typeof(AdminBusinessDetailDto), StatusCodes.Status201Created)]
    [RequirePermission(Permission.CreateBusiness)]
    public async Task<IActionResult> CreateBusiness([FromBody] CreateBusinessInputDto input)
    {
        var command = new CreateBusinessCommand
        {
            PlaceId = input.PlaceId,
            OwnerId = input.OwnerId,
            IsVerified = input.IsVerified,
        };

        var result = await mediator.Send(command);
        return CreatedAtAction(nameof(GetSpecificBusinessDetails), new { businessId = result.Id }, result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedApiResponse<AdminBusinessListDto>), StatusCodes.Status200OK)]
    [RequirePermission(Permission.ViewAllBusinesses)]
    public async Task<IActionResult> GetBusinessesForAdmin([FromQuery] AdminBusinessInputDto input)
    {
        var query = new GetBusinessesForAdminQuery
        {
            Page = input.Page,
            PageSize = input.PageSize,
            Search = input.Search,
            Filters = input.Filters,
            SortBy = input.SortBy ?? "ReviewCount",
            SortDirection = input.SortDirection
        };

        var result = await mediator.Send(query);
        return PagedSuccess(result);
    }

    [HttpPost("{businessId:long}/boost")]
    [ProducesResponseType(typeof(BusinessBoostResultDto), StatusCodes.Status200OK)]
    [RequirePermission(Permission.ManageBusinessPriority)]
    public async Task<IActionResult> BoostSpecificBusiness(long businessId, [FromBody] BoostBusinessInputDto input)
    {
        var command = new BoostSpecificBusinessCommand
        {
            BusinessId = businessId,
            Reason = input.Reason,
            TargetReviews = input.TargetReviews ?? 15
        };

        var result = await mediator.Send(command);
        return Success(result);
    }

    [HttpPost("{businessId:long}/unboost")]
    [ProducesResponseType(typeof(BusinessBoostResultDto), StatusCodes.Status200OK)]
    [RequirePermission(Permission.ManageBusinessPriority)]
    public async Task<IActionResult> UnboostSpecificBusiness(long businessId,
        [FromBody] UnboostBusinessInputDto input)
    {
        var command = new UnboostSpecificBusinessCommand
        {
            BusinessId = businessId,
            Reason = input.Reason ?? "Admin removed boost"
        };

        var result = await mediator.Send(command);
        return Success(result);
    }

    [HttpGet("{businessId:long}")]
    [ProducesResponseType(typeof(AdminBusinessDetailDto), StatusCodes.Status200OK)]
    [RequirePermission(Permission.ViewAllBusinesses)]
    public async Task<IActionResult> GetSpecificBusinessDetails(long businessId)
    {
        var query = new GetSpecificBusinessDetailsQuery { BusinessId = businessId };
        var result = await mediator.Send(query);
        return Success(result);
    }
}