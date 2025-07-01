using AutoMapper;
using MediatR;
using RateRelay.Application.DTOs.Tickets.Queries;
using RateRelay.Application.DTOs.User.Tickets.Queries;
using RateRelay.Domain.Exceptions;
using RateRelay.Domain.Interfaces;

namespace RateRelay.Application.Features.Tickets.Queries.GetTicket;

public class GetTicketQueryHandler(
    ICurrentUserDataResolver userDataResolver,
    ITicketService ticketService,
    IMapper mapper
) : IRequestHandler<GetTicketQuery, GetTicketDetailsOutputDto>
{
    public async Task<GetTicketDetailsOutputDto> Handle(GetTicketQuery request, CancellationToken cancellationToken)
    {
        var userId = userDataResolver.GetAccountId();
        var ticket = await ticketService.GetTicketByIdAsync(
            request.Id,
            userId,
            request.IncludeComments,
            request.IncludeHistory,
            cancellationToken);

        if (ticket is null)
            throw new NotFoundException($"Ticket with ID {request.Id} not found.");

        return mapper.Map<GetTicketDetailsOutputDto>(ticket);
    }
}