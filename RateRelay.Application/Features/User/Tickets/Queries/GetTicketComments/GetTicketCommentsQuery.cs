using MediatR;
using RateRelay.Application.DTOs.Tickets;
using RateRelay.Application.DTOs.User.Tickets;

namespace RateRelay.Application.Features.Tickets.Queries.GetTicketComments;

public class GetTicketCommentsQuery: IRequest<List<TicketCommentDto>>
{
    public long TicketId { get; set; }
    public bool IncludeInternal { get; set; } = false;
}