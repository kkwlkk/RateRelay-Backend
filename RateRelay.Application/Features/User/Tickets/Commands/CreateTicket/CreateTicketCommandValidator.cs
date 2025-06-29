using FluentValidation;
using RateRelay.Application.Features.User.Tickets.Commands.CreateTicket;

namespace RateRelay.Application.Features.Tickets.Commands.CreateTicket;

public class CreateTicketCommandValidator : AbstractValidator<CreateTicketCommand>
{
    public CreateTicketCommandValidator()
    {
        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Invalid ticket type.");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required.")
            .MaximumLength(64)
            .WithMessage("Title cannot exceed 64 characters.");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required.")
            .MaximumLength(2048)
            .WithMessage("Description cannot exceed 2048 characters.");
    }
}   