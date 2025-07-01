using MediatR;

namespace RateRelay.Application.Features.User.Tickets.Commands.CloseTicket;

public class CloseTicketCommand : IRequest<bool>
{
    public long TicketId { get; set; }
    public string Reason { get; set; } = string.Empty;
}