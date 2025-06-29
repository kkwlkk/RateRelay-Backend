using FluentValidation;
using RateRelay.Application.Extensions;

namespace RateRelay.Application.Features.Onboarding.Commands.CompleteBusinessVerificationStep;

public class BusinessVerificationStepCommandValidator : AbstractValidator<CompleteBusinessVerificationStepCommand>
{
    public BusinessVerificationStepCommandValidator()
    {
        RuleFor(x => x.PlaceId)
            .NotEmpty()
            .WithMessage("Google Place ID is required.")
            .WithAppErrorCode("PLACE_ID_REQUIRED");

        RuleFor(x => x.PlaceId)
            .Matches("^[A-Za-z0-9_-]+$")
            .When(x => !string.IsNullOrEmpty(x.PlaceId))
            .WithMessage("Invalid Place ID format.")
            .WithAppErrorCode("INVALID_PLACE_ID");
    }
}