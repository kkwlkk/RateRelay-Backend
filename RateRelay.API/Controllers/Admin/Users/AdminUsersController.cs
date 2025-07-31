using MediatR;
using Microsoft.AspNetCore.Mvc;
using RateRelay.API.Attributes.Auth;
using RateRelay.Application.DTOs.Admin.Users;
using RateRelay.Application.Features.Admin.Users.Queries.GetUsersForAdmin;
using RateRelay.Domain.Common;
using RateRelay.Domain.Enums;

namespace RateRelay.API.Controllers.Admin.Users;

[ApiController]
[Route("api/admin/users")]
[Area("Admin")]
public class AdminUsersController(IMediator mediator) : AdminBaseController
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedApiResponse<AdminUserListDto>), StatusCodes.Status200OK)]
    [RequirePermission(Permission.ViewAllUsers)]
    public async Task<IActionResult> GetUsers([FromQuery] AdminGetUsersInputDto input)
    {
        var query = new GetUsersForAdminQuery
        {
            IsVerified = input.Filters?.IsVerified,
            Page = input.Page,
            PageSize = input.PageSize,
            Search = input.Search,
            SortBy = input.SortBy,
            SortDirection = input.SortDirection
        };

        var result = await mediator.Send(query);
        return Ok(result);
    }
}