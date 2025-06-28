using AutoMapper;
using MediatR;
using RateRelay.Application.DTOs.Tickets;
using RateRelay.Domain.Interfaces;

namespace RateRelay.Application.Features.Tickets.Queries.GetTicketComments;

public class GetTicketCommentsQueryHandler(
    ICurrentUserDataResolver currentUserDataResolver,
    ITicketService ticketService,
    IMapper mapper
) : IRequestHandler<GetTicketCommentsQuery, List<TicketCommentDto>>
{
    public async Task<List<TicketCommentDto>> Handle(GetTicketCommentsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = currentUserDataResolver.GetAccountId();
        var comments = await ticketService.GetTicketCommentsAsync(
            request.TicketId,
            userId,
            request.IncludeInternal,
            cancellationToken: cancellationToken
        );

        return mapper.Map<List<TicketCommentDto>>(comments);
    }
}