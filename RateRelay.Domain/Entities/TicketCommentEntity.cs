using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RateRelay.Domain.Entities;

[Table("ticket_comments")]
[Index(nameof(TicketId), Name = "IX_TicketComments_TicketId")]
[Index(nameof(AuthorId), Name = "IX_TicketComments_AuthorId")]
[Index(nameof(TicketId), nameof(AuthorId), Name = "IX_TicketComments_TicketId_AuthorId")]
public class TicketCommentEntity : BaseEntity
{
    public long TicketId { get; set; }

    [ForeignKey("TicketId")]
    public virtual TicketEntity Ticket { get; set; } = null!;

    public long AuthorId { get; set; }
    
    [ForeignKey("AuthorId")]
    public virtual AccountEntity Author { get; set; } = null!;
    
    [MaxLength(2048)]
    public string Content { get; set; } = string.Empty;
    
    public bool IsInternal { get; set; } = false;
    public bool IsSystemGenerated { get; set; } = false;
}