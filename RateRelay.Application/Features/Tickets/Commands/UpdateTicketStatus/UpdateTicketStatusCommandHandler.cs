using MediatR;
using RateRelay.Domain.Interfaces;

namespace RateRelay.Application.Features.Tickets.Commands.UpdateTicketStatus;

public class UpdateTicketStatusCommandHandler(
    ICurrentUserDataResolver currentUserDataResolver,
    ITicketService ticketService
) : IRequestHandler<UpdateTicketStatusCommand, bool>
{
    public async Task<bool> Handle(UpdateTicketStatusCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserDataResolver.GetAccountId();
        var ticket =
            await ticketService.GetTicketByIdAsync(request.TicketId, userId, cancellationToken: cancellationToken);
        if (ticket == null) return false;

        await ticketService.UpdateTicketStatusAsync(
            ticket.Id,
            request.Status,
            userId,
            request.Comment,
            cancellationToken: cancellationToken
        );
        return true;
    }
}