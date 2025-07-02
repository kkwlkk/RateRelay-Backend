using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RateRelay.Application.DTOs.User.Tickets.Queries;
using RateRelay.Domain.Common;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Enums;
using RateRelay.Domain.Interfaces;
using RateRelay.Domain.Interfaces.DataAccess;

namespace RateRelay.Application.Features.User.Tickets.Queries.GetUserTickets;

public class GetUserTicketsQueryHandler(
    ICurrentUserDataResolver currentUserDataResolver,
    IUnitOfWorkFactory unitOfWorkFactory,
    IMapper mapper
) : IRequestHandler<GetUserTicketsQuery, PagedApiResponse<GetUserTicketsOutputDto>>
{
    public async Task<PagedApiResponse<GetUserTicketsOutputDto>> Handle(GetUserTicketsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = currentUserDataResolver.GetAccountId();
        await using var uow = await unitOfWorkFactory.CreateAsync();
        var ticketRepository = uow.GetRepository<TicketEntity>();

        var query = ticketRepository.GetBaseQueryable()
            .Include(t => t.Reporter)
            .Include(t => t.AssignedTo)
            .Where(t => t.ReporterId == userId || t.AssignedToId == userId)
            .Where(t => t.Status != TicketStatus.Obsolete);

        if (request.Type.HasValue)
            query = query.Where(t => t.Type == request.Type.Value);

        if (request.Status.HasValue)
            query = query.Where(t => t.Status == request.Status.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var tickets = await query
            .OrderByDescending(t => t.LastActivityUtc)
            .ApplyPaging(request)
            .ApplySearch(request, x => x.Title.Contains(request.Search!) || x.Description.Contains(request.Search!))
            .ApplySorting(request)
            .ToListAsync(cancellationToken: cancellationToken);

        var mappedTickets = mapper.Map<IEnumerable<GetUserTicketsOutputDto>>(tickets);

        return request.ToPagedResponse(
            mappedTickets,
            totalCount
        );
    }
}