using AutoMapper;
using MediatR;
using RateRelay.Application.DTOs.Tickets.Commands;
using RateRelay.Domain.Interfaces;

namespace RateRelay.Application.Features.Tickets.Commands.CreateTicket;

public class CreateTicketCommandHandler(
    ITicketService ticketService,
    ICurrentUserDataResolver currentUserDataResolver,
    IMapper mapper)
    : IRequestHandler<CreateTicketCommand, CreateTicketOutputDto>
{
    public async Task<CreateTicketOutputDto> Handle(CreateTicketCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = currentUserDataResolver.GetAccountId();
        var ticket = await ticketService.CreateTicketAsync(
            request.Type, request.Title, request.Description, currentUserId,
            null, null, cancellationToken);

        return mapper.Map<CreateTicketOutputDto>(ticket);
    }
}