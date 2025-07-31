using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RateRelay.Domain.Entities;

[Table("business_boosts")]
[Index(nameof(BusinessId))]
[Index(nameof(IsActive))]
public class BusinessBoostEntity : BaseEntity
{
    public long BusinessId { get; set; }
        
    [ForeignKey("BusinessId")]
    public virtual BusinessEntity Business { get; set; } = null!;
    
    public int TargetReviews { get; set; }
    public int ReviewsAtBoostStart { get; set; }
    public DateTime BoostedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsActive { get; set; } = true;
    
    public long? CreatedById { get; set; }
    [ForeignKey("CreatedById")]
    public virtual AccountEntity CreatedBy { get; set; } = null!;
    
    public bool CreatedBySystem { get; set; } = false;
    
    [MaxLength(255)]
    public string? Reason { get; set; }
}