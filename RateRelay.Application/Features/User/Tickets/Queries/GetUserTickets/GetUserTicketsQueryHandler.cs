using AutoMapper;
using MediatR;
using RateRelay.Application.DTOs.User.Tickets.Queries;
using RateRelay.Domain.Common;
using RateRelay.Domain.Interfaces;
using Serilog;

namespace RateRelay.Application.Features.User.Tickets.Queries.GetUserTickets;

public class GetUserTicketsQueryHandler(
    ICurrentUserDataResolver currentUserDataResolver,
    ITicketService ticketService,
    IMapper mapper,
    ILogger logger
) : IRequestHandler<GetUserTicketsQuery, PagedApiResponse<GetUserTicketsOutputDto>>
{
    public async Task<PagedApiResponse<GetUserTicketsOutputDto>> Handle(GetUserTicketsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = currentUserDataResolver.GetAccountId();
        var tickets = await ticketService.GetPagedUserTicketsAsync(
            request.Page,
            request.PageSize,
            userId,
            statusFilter: request.Status,
            typeFilter: request.Type,
            cancellationToken: cancellationToken
        );

        logger.Information(
            "Retrieved {Count} tickets for user {UserId} with status {Status} and type {Type}",
            tickets.TotalCount,
            userId,
            request.Status,
            request.Type
        );

        var mappedTickets = mapper.Map<IEnumerable<GetUserTicketsOutputDto>>(tickets.Items);

        return request.ToPagedResponse(
            mappedTickets,
            tickets.TotalCount
        );
    }
}