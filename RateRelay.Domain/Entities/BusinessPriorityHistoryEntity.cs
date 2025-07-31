using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RateRelay.Domain.Entities;

[Table("business_priority_history")]
[Index(nameof(BusinessId))]
[Index(nameof(ChangedById))]
public class BusinessPriorityHistoryEntity : BaseEntity
{
    public long BusinessId { get; set; }
        
    [ForeignKey("BusinessId")]
    public virtual BusinessEntity Business { get; set; } = null!;

    public byte OldPriority { get; set; }
    public byte NewPriority { get; set; }
        
    public long? ChangedById { get; set; }
        
    [ForeignKey("ChangedById")]
    public virtual AccountEntity ChangedBy { get; set; } = null!;
    
    public bool ChangedBySystem { get; set; } = false;

    [MaxLength(255)]
    public string? Reason { get; set; }
}