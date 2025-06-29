using MediatR;
using RateRelay.Application.DTOs.Tickets.Commands;

namespace RateRelay.Application.Features.Tickets.Commands.AddTicketComment;

public class AddTicketCommentCommand : IRequest<AddTicketCommentOutputDto>
{
    public long TicketId { get; set; }
    public string Comment { get; set; } = string.Empty;
    public bool IsInternal { get; set; } = false;
}