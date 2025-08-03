using MediatR;
using RateRelay.Application.DTOs.Admin.Users.Queries.GetUsersForAdmin;
using RateRelay.Domain.Common;

namespace RateRelay.Application.Features.Admin.Users.Queries.GetUsersForAdmin;

public class GetUsersForAdminQuery : PagedRequest, IRequest<PagedApiResponse<AdminUserListDto>>
{
    public bool? IsVerified { get; set; }
}