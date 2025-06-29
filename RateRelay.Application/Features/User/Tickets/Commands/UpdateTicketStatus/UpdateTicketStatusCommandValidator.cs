using FluentValidation;

namespace RateRelay.Application.Features.Tickets.Commands.UpdateTicketStatus;

public class UpdateTicketStatusCommandValidator : AbstractValidator<UpdateTicketStatusCommand>
{
    public UpdateTicketStatusCommandValidator()
    {
        RuleFor(x => x.TicketId)
            .GreaterThan(0)
            .WithMessage("Ticket ID must be greater than 0.");

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Invalid ticket status.");

        RuleFor(x => x.Comment)
            .MaximumLength(256)
            .WithMessage("Comment cannot exceed 256 characters.")
            .When(x => !string.IsNullOrEmpty(x.Comment));
    }
}