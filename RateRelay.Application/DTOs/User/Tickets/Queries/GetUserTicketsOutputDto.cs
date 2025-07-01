using RateRelay.Domain.Common;
using RateRelay.Domain.Enums;

namespace RateRelay.Application.DTOs.User.Tickets.Queries;

public class GetUserTicketsOutputDto
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketType Type { get; set; }
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? LastActivityAtUtc { get; set; }
    public long ReporterId { get; set; }
    public string ReporterName { get; set; } = string.Empty;
    public bool IsAssigned { get; set; }
    public string? AssignedToName { get; set; }
    public TicketSubjects Subjects { get; set; }
}