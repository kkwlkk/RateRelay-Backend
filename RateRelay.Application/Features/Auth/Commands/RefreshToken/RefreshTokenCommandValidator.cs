using FluentValidation;
using RateRelay.Application.Extensions;

namespace RateRelay.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token is required.")
            .WithAppErrorCode("REFRESH_TOKEN_REQUIRED")
            .MaximumLength(256)
            .WithMessage("Refresh token must be at most 256 characters long.")
            .WithAppErrorCode("REFRESH_TOKEN_TOO_LONG");
    }
}