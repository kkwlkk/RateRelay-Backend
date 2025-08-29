using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RateRelay.Domain.Entities;

[Table("maintenance_mode")]
public class MaintenanceModeEntity : BaseEntity
{
    public bool IsActive { get; set; }

    public long CreatedByAccountId { get; set; }

    [ForeignKey("CreatedByAccountId")]
    public virtual AccountEntity CreatedByAccount { get; set; } = null!;

    [MaxLength(500)]
    public string? Reason { get; set; }
}