using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RateRelay.Domain.Entities;

[Table("roles")]
[Index(nameof(Name), IsUnique = true)]
public class RoleEntity : BaseEntity
{
    [MaxLength(64)]
    public required string Name { get; set; }
    
    [MaxLength(255)]
    public string? Description { get; set; }

    public ulong Permissions { get; set; }

    [Column("is_hidden")]
    public bool IsHidden { get; set; }
    
    [InverseProperty("Role")]
    public ICollection<AccountEntity> Accounts { get; set; } = new List<AccountEntity>();
}