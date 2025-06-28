using RateRelay.Domain.Common;
using RateRelay.Domain.Enums;

namespace RateRelay.Application.DTOs.Tickets.Queries;

public class GetUserTicketsInputDto : PagedRequest
{
    public TicketType? Type { get; set; }
    public TicketStatus? Status { get; set; }
}