using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using RateRelay.Domain.Enums;

namespace RateRelay.Domain.Entities;

[Table("tickets")]
[Index(nameof(Status), Name = "IX_Tickets_Status")]
[Index(nameof(Type), Name = "IX_Tickets_Type")]
[Index(nameof(ReporterId), Name = "IX_Tickets_ReporterId")]
[Index(nameof(AssignedToId), Name = "IX_Tickets_AssignedToId")]
[Index(nameof(Status), nameof(AssignedToId), Name = "IX_Tickets_Status_AssignedToId")]
[Index(nameof(Status), nameof(LastActivityUtc), Name = "IX_Tickets_Status_LastActivityUtc")]
public class TicketEntity : BaseEntity
{
    public TicketStatus Status { get; set; } = TicketStatus.Open;

    public TicketType Type { get; set; }

    public long ReporterId { get; set; }

    [ForeignKey("ReporterId")]
    public virtual AccountEntity Reporter { get; set; } = null!;

    public long? AssignedToId { get; set; }

    [ForeignKey("AssignedToId")]
    public virtual AccountEntity? AssignedTo { get; set; }

    [MaxLength(64)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2048)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(4096)]
    public string InternalNotes { get; set; } = string.Empty;

    public DateTime? DateStartedUtc { get; set; }
    public DateTime? DateResolvedUtc { get; set; }
    public DateTime? DateClosedUtc { get; set; }
    public DateTime LastActivityUtc { get; set; } = DateTime.UtcNow;

    public long? SubjectBusinessId { get; set; }

    [ForeignKey("SubjectBusinessId")]
    public virtual BusinessEntity? SubjectBusiness { get; set; }

    public long? SubjectReviewId { get; set; }

    [ForeignKey("SubjectReviewId")]
    public virtual BusinessReviewEntity? SubjectReview { get; set; }

    public virtual ICollection<TicketCommentEntity> Comments { get; set; } = new HashSet<TicketCommentEntity>();

    public virtual ICollection<TicketStatusHistoryEntity> StatusHistory { get; set; } =
        new HashSet<TicketStatusHistoryEntity>();

    [NotMapped]
    public bool IsOpen => Status is TicketStatus.Open or TicketStatus.InProgress or TicketStatus.Reopened;

    [NotMapped]
    public bool IsAssigned => AssignedToId.HasValue && AssignedToId != 0;

    [NotMapped]
    public bool IsResolved => Status is TicketStatus.Resolved or TicketStatus.Closed;

    [NotMapped]
    public bool IsObsolete => Status == TicketStatus.Obsolete;

    [NotMapped]
    public bool IsOnHold => Status == TicketStatus.OnHold;
}