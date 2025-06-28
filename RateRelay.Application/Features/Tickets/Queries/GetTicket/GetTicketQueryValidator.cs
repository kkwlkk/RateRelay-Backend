using FluentValidation;

namespace RateRelay.Application.Features.Tickets.Queries.GetTicket;

public class GetTicketQueryValidator : AbstractValidator<GetTicketQuery>
{
    public GetTicketQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Ticket ID must be greater than zero.");
    }
}