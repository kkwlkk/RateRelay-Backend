using FluentValidation;
using RateRelay.Infrastructure.Extensions;

namespace RateRelay.Application.Features.Auth.Commands.Google;

public class GoogleAuthCommandValidator : AbstractValidator<GoogleAuthCommand>
{
    public GoogleAuthCommandValidator()
    {
        RuleFor(x => x.OAuthIdToken)
            .NotEmpty()
            .WithMessage("OAuthIdToken is required")
            .Must(x => x.IsValidJwtFormat())
            .WithMessage("OAuthIdToken is not a valid JWT");
    }
}