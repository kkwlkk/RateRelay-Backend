using FluentValidation;
using RateRelay.Application.Extensions;
using RateRelay.Application.Features.Onboarding.Commands.CompleteProfileSetupStep;

namespace RateRelay.Application.Features.User.Onboarding.Commands.CompleteProfileSetupStep;

public class CompleteProfileSetupCommandValidator : AbstractValidator<CompleteProfileSetupStepCommand>
{
    public CompleteProfileSetupCommandValidator()
    {
        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .WithMessage("Display name is required.")
            .WithAppErrorCode("DISPLAY_NAME_REQUIRED")
            .MinimumLength(3)
            .WithMessage("Display name must be at least 3 characters long.")
            .WithAppErrorCode("DISPLAY_NAME_TOO_SHORT")
            .MaximumLength(64)
            .WithMessage("Display name must be less than 64 characters long.")
            .WithAppErrorCode("DISPLAY_NAME_TOO_LONG")
            .Matches(@"^[a-zA-Z0-9\s\-_\.']+$")
            .WithMessage("Display name can only contain alphanumeric characters, spaces, and - _ .'.");
    }
}