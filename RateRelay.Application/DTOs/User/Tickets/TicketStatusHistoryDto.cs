using RateRelay.Domain.Enums;

namespace RateRelay.Application.DTOs.User.Tickets;

public class TicketStatusHistoryDto
{
    public DateTime DateCreatedUtc { get; set; }
    public TicketStatus? FromStatus { get; set; }
    public TicketStatus ToStatus { get; set; }
    public string ChangedByName { get; set; } = string.Empty;
    public string? ChangedReason { get; set; }
}