using FluentValidation;
using RateRelay.Application.Extensions;

namespace RateRelay.Application.Features.Onboarding.Commands.CompleteProfileSetupStep;

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
            .WithMessage("Display name must be less than 65 characters long.")
            .WithAppErrorCode("DISPLAY_NAME_TOO_LONG");
    }
}