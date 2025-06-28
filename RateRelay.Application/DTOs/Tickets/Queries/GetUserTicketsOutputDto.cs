using RateRelay.Domain.Enums;

namespace RateRelay.Application.DTOs.Tickets.Queries;

public class GetUserTicketsOutputDto
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketType Type { get; set; }
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastActivityUtc { get; set; }
    public long AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string AssignedToName { get; set; } = string.Empty;
}