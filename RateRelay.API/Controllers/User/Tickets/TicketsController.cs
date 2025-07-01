using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RateRelay.Application.DTOs.Tickets.Commands;
using RateRelay.Application.DTOs.Tickets.Queries;
using RateRelay.Application.DTOs.User.Tickets.Commands;
using RateRelay.Application.DTOs.User.Tickets.Queries;
using RateRelay.Application.Features.Tickets.Commands.AddTicketComment;
using RateRelay.Application.Features.Tickets.Commands.CreateTicket;
using RateRelay.Application.Features.Tickets.Queries.GetTicket;
using RateRelay.Application.Features.Tickets.Queries.GetTicketComments;
using RateRelay.Application.Features.User.Tickets.Commands.CloseTicket;
using RateRelay.Application.Features.User.Tickets.Commands.CreateTicket;
using RateRelay.Application.Features.User.Tickets.Queries.GetUserTickets;
using RateRelay.Domain.Common;

namespace RateRelay.API.Controllers.User.Tickets;

[ApiController]
[Authorize]
[Area("User Tickets")]
public class TicketsController(IMediator mediator, IMapper mapper) : UserBaseController
{
    [HttpPost]
    public async Task<IActionResult> CreateTicket(CreateTicketCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return Success(result, StatusCodes.Status201Created);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedApiResponse<GetUserTicketsOutputDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserTickets([FromQuery] GetUserTicketsInputDto input,
        CancellationToken cancellationToken)
    {
        var query = mapper.Map<GetUserTicketsQuery>(input);
        var result = await mediator.Send(query, cancellationToken);
        return PagedSuccess(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<GetTicketDetailsOutputDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTicket(long id, [FromQuery] bool includeComments = false,
        [FromQuery] bool includeHistory = false, CancellationToken cancellationToken = default)
    {
        var query = new GetTicketQuery { Id = id, IncludeComments = includeComments, IncludeHistory = includeHistory };
        var result = await mediator.Send(query, cancellationToken);
        return Success(result);
    }

    [HttpGet("{id}/comments")]
    public async Task<IActionResult> GetTicketComments(long id, CancellationToken cancellationToken)
    {
        var query = new GetTicketCommentsQuery { TicketId = id };
        var result = await mediator.Send(query, cancellationToken);
        return Success(result);
    }

    [HttpPost("{id}/comments")]
    public async Task<ActionResult<AddTicketCommentOutputDto>> AddComment(long id, AddTicketCommandInputDto input,
        CancellationToken cancellationToken)
    {
        var command = mapper.Map<AddTicketCommentCommand>(input);
        command.TicketId = id;
        var result = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetTicket), new { id }, result);
    }

    [HttpPut("{id}/close")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CloseTicket(long id, [FromBody] CloseTicketCommandInputDto input,
        CancellationToken cancellationToken)
    {
        var command = mapper.Map<CloseTicketCommand>(input);
        command.TicketId = id;
        await mediator.Send(command, cancellationToken);
        return Success(statusCode: 204);
    }

    // [HttpPut("{id}/status")]
    // [RequirePermission(Permission.ChangeTicketStatus)]
    // public async Task<ActionResult> UpdateTicketStatus(long id, UpdateTicketStatusInputDto command,
    //     CancellationToken cancellationToken)
    // {
    //     var updateCommand = mapper.Map<UpdateTicketStatusCommand>(command);
    //     updateCommand.TicketId = id;
    //     var result = await mediator.Send(updateCommand, cancellationToken);
    //     return result ? NoContent() : NotFound();
    // }

    // [HttpPut("{id}/assignment")]
    // public async Task<ActionResult> AssignTicket(long id, AssignTicketCommand command,
    //     CancellationToken cancellationToken)
    // {
    //     command.TicketId = id;
    //     var result = await mediator.Send(command, cancellationToken);
    //     return result ? NoContent() : NotFound();
    // }

    // [HttpPut("{id}/obsolete")]
    // public async Task<ActionResult> MarkObsolete(long id, MarkTicketObsoleteCommand command,
    //     CancellationToken cancellationToken)
    // {
    //     command.TicketId = id;
    //     var result = await mediator.Send(command, cancellationToken);
    //     return result ? NoContent() : NotFound();
    // }
    //
    // [HttpGet("user/{userId}")]
    // public async Task<ActionResult<IEnumerable<TicketSummaryResponse>>> GetUserTickets(long userId,
    //     [FromQuery] bool includeAssigned = true, [FromQuery] bool includeReported = true,
    //     [FromQuery] TicketStatus? status = null, CancellationToken cancellationToken = default)
    // {
    //     var query = new GetUserTicketsQuery
    //     {
    //         UserId = userId,
    //         IncludeAssigned = includeAssigned,
    //         IncludeReported = includeReported,
    //         StatusFilter = status
    //     };
    //     var result = await mediator.Send(query, cancellationToken);
    //     return Ok(result);
    // }
    //
    // [HttpGet]
    // public async Task<ActionResult<PagedResult<TicketSummaryResponse>>> GetTickets([FromQuery] GetTicketsQuery query,
    //     CancellationToken cancellationToken)
    // {
    //     var result = await mediator.Send(query, cancellationToken);
    //     return Ok(result);
    // }
}