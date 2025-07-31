using MediatR;
using RateRelay.Application.DTOs.Admin.Users;
using RateRelay.Domain.Common;

namespace RateRelay.Application.Features.Admin.Users.Queries.GetUsersForAdmin;

public class GetUsersForAdminQuery : PagedRequest, IRequest<PagedApiResponse<AdminUserListDto>>
{
    public bool? IsVerified { get; set; }
}