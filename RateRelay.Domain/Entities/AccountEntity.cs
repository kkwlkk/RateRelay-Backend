using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RateRelay.Domain.Entities;

[Table("accounts")]
[Index(nameof(Username), IsUnique = true)]
public class AccountEntity : BaseEntity
{
    [MaxLength(64)]
    public required string Username { get; set; }

    [MaxLength(255)]
    public required string PasswordHash { get; set; }

    public ulong Permissions { get; set; }

    public long? RoleId { get; set; }

    [ForeignKey("RoleId")]
    public RoleEntity? Role { get; set; }
}