using MediatR;
using RateRelay.Domain.Exceptions;
using RateRelay.Domain.Interfaces;
using RateRelay.Infrastructure.Services;

namespace RateRelay.Application.Features.User.Tickets.Commands.CloseTicket;

public class CloseTicketCommandHandler(
    CurrentUserContext userContext,
    ITicketService ticketService
) : IRequestHandler<CloseTicketCommand, bool>
{
    public async Task<bool> Handle(CloseTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await ticketService.GetTicketByIdAsync(request.TicketId, userContext.AccountId,
            cancellationToken: cancellationToken);

        if (ticket is null)
        {
            throw new NotFoundException($"Ticket with ID {request.TicketId} not found.", "TicketNotFound");
        }

        if (ticket.Status != Domain.Enums.TicketStatus.Open)
        {
            throw new AppException($"Ticket with ID {request.TicketId} is not open and cannot be closed.", "TicketAlreadyClosed");
        }
        
        return await ticketService.UpdateTicketStatusAsync(
            request.TicketId,
            Domain.Enums.TicketStatus.Closed,
            userContext.AccountId,
            request.Reason,
            cancellationToken: cancellationToken
        );
    }
}