namespace RateRelay.Application.DTOs.Admin.Users.Queries.GetUsersForAdmin;

public class AdminUserListDto
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DateCreatedUtc { get; set; }
    public bool IsVerified { get; set; }
}