using RateRelay.Domain.Common;

namespace RateRelay.Application.DTOs.Admin.Users;

// TODO: cleanup and move to their own files

public class AdminUserListDto
{
    public long Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string GoogleUsername { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DateCreatedUtc { get; set; }
    public bool IsVerified { get; set; }
}

public class AdminUserFilterDto
{
    public bool? IsVerified { get; set; }
}

public class AdminGetUsersInputDto : PagedRequest<AdminUserFilterDto>;