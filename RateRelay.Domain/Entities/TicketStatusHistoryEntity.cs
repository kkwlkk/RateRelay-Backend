using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using RateRelay.Domain.Enums;

namespace RateRelay.Domain.Entities;

[Table("ticket_status_history")]
[Index(nameof(TicketId), Name = "IX_TicketStatusHistory_TicketId")]
[Index(nameof(FromStatus), Name = "IX_TicketStatusHistory_FromStatus")]
[Index(nameof(ToStatus), Name = "IX_TicketStatusHistory_ToStatus")]
[Index(nameof(ChangedById), Name = "IX_TicketStatusHistory_ChangedById")]
public class TicketStatusHistoryEntity : BaseEntity
{
    public long TicketId { get; set; }

    [ForeignKey("TicketId")]
    public virtual TicketEntity Ticket { get; set; } = null!;

    public TicketStatus? FromStatus { get; set; }
    
    public TicketStatus ToStatus { get; set; }

    public long ChangedById { get; set; }

    [ForeignKey("ChangedById")]
    public virtual AccountEntity ChangedBy { get; set; } = null!;
    
    [MaxLength(256)]
    public string? ChangedReason { get; set; } = null;
}