using AutoMapper;
using MediatR;
using RateRelay.Application.DTOs.Tickets.Commands;
using RateRelay.Application.DTOs.User.Tickets.Commands;
using RateRelay.Domain.Exceptions;
using RateRelay.Domain.Interfaces;

namespace RateRelay.Application.Features.Tickets.Commands.AddTicketComment;

public class AddTicketCommentCommandHandler(
    ICurrentUserDataResolver currentUserDataResolver,
    ITicketService ticketService,
    IMapper mapper
)
    : IRequestHandler<AddTicketCommentCommand, AddTicketCommentOutputDto>
{
    public async Task<AddTicketCommentOutputDto> Handle(AddTicketCommentCommand request,
        CancellationToken cancellationToken)
    {
        var userId = currentUserDataResolver.GetAccountId();

        var ticket =
            await ticketService.GetTicketByIdAsync(request.TicketId, userId, cancellationToken: cancellationToken);
        if (ticket == null)
        {
            throw new NotFoundException($"Ticket with ID {request.TicketId} not found.");
        }

        var comment = await ticketService.AddCommentAsync(
            request.TicketId,
            userId,
            request.Comment,
            request.IsInternal,
            cancellationToken: cancellationToken
        );

        return mapper.Map<AddTicketCommentOutputDto>(comment);
    }
}