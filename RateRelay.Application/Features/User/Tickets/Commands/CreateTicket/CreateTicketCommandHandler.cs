using AutoMapper;
using MediatR;
using RateRelay.Application.DTOs.Tickets.Commands;
using RateRelay.Application.Features.Tickets.Commands.CreateTicket;
using RateRelay.Domain.Exceptions;
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

        var isUserOnTicketCooldown =
            await ticketService.IsUserOnTicketCooldownAsync(currentUserId, request.Type, cancellationToken);

        if (isUserOnTicketCooldown)
            throw new AppException("User is on cooldown for creating tickets of this type.", "TicketCooldown");

        var ticket = await ticketService.CreateTicketAsync(
            request.Type, request.Title, request.Description, currentUserId,
            cancellationToken: cancellationToken);

        return mapper.Map<CreateTicketOutputDto>(ticket);
    }
}