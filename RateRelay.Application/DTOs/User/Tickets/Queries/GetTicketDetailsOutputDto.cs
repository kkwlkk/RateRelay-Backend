using RateRelay.Application.DTOs.Tickets;
using RateRelay.Domain.Common;
using RateRelay.Domain.Enums;

namespace RateRelay.Application.DTOs.User.Tickets.Queries;

public class GetTicketDetailsOutputDto
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketType Type { get; set; }
    public TicketStatus Status { get; set; }

    public DateTime DateCreatedUtc { get; set; }
    public DateTime? LastActivityUtc { get; set; }

    public long ReporterId { get; set; }
    public string ReporterName { get; set; } = string.Empty;
    public bool IsAssigned { get; set; }
    public string AssignedToName { get; set; } = string.Empty;
    public bool IsOpen { get; set; }
    public bool IsResolved { get; set; }

    public TicketSubjects Subjects { get; set; }
    public List<TicketCommentDto> Comments { get; set; } = [];
    public List<TicketStatusHistoryDto> StatusHistory { get; set; } = [];
}