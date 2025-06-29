using MediatR;
using RateRelay.Application.DTOs.Tickets.Queries;

namespace RateRelay.Application.Features.Tickets.Queries.GetTicket;

public class GetTicketQuery : IRequest<GetTicketDetailsOutputDto>
{
    public long Id { get; set; }
    public bool IncludeComments { get; set; } = false;
    public bool IncludeHistory { get; set; } = false;
}