using MediatR;
using RateRelay.Application.DTOs.Tickets.Commands;
using RateRelay.Domain.Enums;

namespace RateRelay.Application.Features.Tickets.Commands.CreateTicket;

public class CreateTicketCommand : IRequest<CreateTicketOutputDto>
{
    public TicketType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}