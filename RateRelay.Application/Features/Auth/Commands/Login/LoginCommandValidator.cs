using FluentValidation;
using RateRelay.Application.Extensions;

namespace RateRelay.Application.Features.Auth.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required.")
            .WithAppErrorCode("USERNAME_REQUIRED")
            .MaximumLength(64)
            .WithMessage("Username must be at most 64 characters long.")
            .WithAppErrorCode("USERNAME_TOO_LONG");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required.")
            .WithAppErrorCode("PASSWORD_REQUIRED");
    }
}