using RateRelay.Domain.Entities;

namespace RateRelay.Application.DTOs.Account.Queries;

public class AccountQueryOutputDto
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public ulong Permissions { get; set; }
    public int PointBalance { get; set; }
    public RoleEntityOutputDto? Role { get; set; }
}