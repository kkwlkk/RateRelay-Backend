using MediatR;
using RateRelay.Application.DTOs.Tickets.Queries;
using RateRelay.Domain.Common;
using RateRelay.Domain.Enums;

namespace RateRelay.Application.Features.Tickets.Queries.GetUserTickets;

public class GetUserTicketsQuery : PagedRequest, IRequest<PagedApiResponse<GetUserTicketsOutputDto>>
{
    public TicketType? Type { get; set; }
    public TicketStatus? Status { get; set; }
}