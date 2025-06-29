using MediatR;
using RateRelay.Domain.Enums;

namespace RateRelay.Application.Features.Tickets.Commands.UpdateTicketStatus;

public class UpdateTicketStatusCommand : IRequest<bool>
{
    public long TicketId { get; set; }
    public TicketStatus Status { get; set; }
    public string? Comment { get; set; }
}