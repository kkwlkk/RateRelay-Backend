using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RateRelay.Domain.Entities;

/// <summary>
/// Base class for all entities, includes common properties for auditing and soft delete.
/// </summary>
public abstract class BaseEntity
{
    [Key]
    public long Id { get; set; }

    public DateTime DateCreatedUtc { get; set; } = DateTime.UtcNow;

    public DateTime? DateModifiedUtc { get; set; }

    public DateTime? DateDeletedUtc { get; set; }

    [NotMapped]
    protected bool IsDeleted => DateDeletedUtc.HasValue;
}