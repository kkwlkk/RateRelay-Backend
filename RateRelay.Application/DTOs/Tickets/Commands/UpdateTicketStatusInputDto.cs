using RateRelay.Domain.Enums;

namespace RateRelay.Application.DTOs.Tickets.Commands;

public class UpdateTicketStatusInputDto
{
    public TicketStatus Status { get; set; }
    public string? Comment { get; set; }
}