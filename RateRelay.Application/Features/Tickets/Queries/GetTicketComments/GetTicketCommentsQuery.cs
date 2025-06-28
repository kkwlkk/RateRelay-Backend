using MediatR;
using RateRelay.Application.DTOs.Tickets;

namespace RateRelay.Application.Features.Tickets.Queries.GetTicketComments;

public class GetTicketCommentsQuery: IRequest<List<TicketCommentDto>>
{
    public long TicketId { get; set; }
    public bool IncludeInternal { get; set; } = false;
}