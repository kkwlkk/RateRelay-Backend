using FluentValidation;
using RateRelay.Application.Extensions;

namespace RateRelay.Application.Features.Accounts.Commands;

public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required.")
            .WithAppErrorCode("USERNAME_REQUIRED")
            .MaximumLength(64)
            .WithMessage("Username must not exceed 64 characters.")
            .WithAppErrorCode("USERNAME_MAX_LENGTH");
    }
}