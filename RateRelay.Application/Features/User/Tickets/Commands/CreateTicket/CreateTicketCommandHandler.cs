using AutoMapper;
using MediatR;
using RateRelay.Application.DTOs.Tickets.Commands;
using RateRelay.Application.Features.Tickets.Commands.CreateTicket;
using RateRelay.Domain.Interfaces;
using RateRelay.Infrastructure.Services;

namespace RateRelay.Application.Features.User.Tickets.Commands.CreateTicket;

public class CreateTicketCommandHandler(
    ITicketService ticketService,
    CurrentUserContext userContext,
    IMapper mapper)
    : IRequestHandler<CreateTicketCommand, CreateTicketOutputDto>
{
    public async Task<CreateTicketOutputDto> Handle(CreateTicketCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = userContext.AccountId;
        var ticket = await ticketService.CreateTicketAsync(
            request.Type, request.Title, request.Description, currentUserId,
            null, null, cancellationToken);

        return mapper.Map<CreateTicketOutputDto>(ticket);
    }
}