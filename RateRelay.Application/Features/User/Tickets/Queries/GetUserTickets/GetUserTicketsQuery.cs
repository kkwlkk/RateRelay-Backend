using MediatR;
using RateRelay.Application.DTOs.User.Tickets.Queries;
using RateRelay.Domain.Common;
using RateRelay.Domain.Enums;

namespace RateRelay.Application.Features.User.Tickets.Queries.GetUserTickets;

public class GetUserTicketsQuery : PagedRequest, IRequest<PagedApiResponse<GetUserTicketsOutputDto>>
{
    public TicketType? Type { get; set; }
    public TicketStatus? Status { get; set; }
}