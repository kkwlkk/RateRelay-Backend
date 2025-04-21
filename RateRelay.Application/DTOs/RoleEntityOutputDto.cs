namespace RateRelay.Application.DTOs;

public class RoleEntityOutputDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ulong Permissions { get; set; }
    public bool IsHidden { get; set; }
}