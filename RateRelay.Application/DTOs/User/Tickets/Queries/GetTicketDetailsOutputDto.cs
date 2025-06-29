using RateRelay.Domain.Enums;

namespace RateRelay.Application.DTOs.Tickets.Queries;

public class GetTicketDetailsOutputDto
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketType Type { get; set; }
    public TicketStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? LastActivityUtc { get; set; }

    public string ReporterName { get; set; } = string.Empty;
    public string AssignedToName { get; set; } = string.Empty;
    
    public bool IsAssigned => !string.IsNullOrEmpty(AssignedToName);
    public bool IsOpen => Status is TicketStatus.Open or TicketStatus.InProgress or TicketStatus.Reopened;
    public bool IsResolved => Status is TicketStatus.Resolved or TicketStatus.Closed;

    public List<TicketCommentDto> Comments { get; set; } = [];
    public List<TicketStatusHistoryDto> StatusHistory { get; set; } = [];
}