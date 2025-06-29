using FluentValidation;
using RateRelay.Application.Extensions;

namespace RateRelay.Application.Features.Onboarding.Commands.CompleteWelcomeStep;

public class WelcomeStepCommandValidator : AbstractValidator<CompleteWelcomeStepCommand>
{
    public WelcomeStepCommandValidator()
    {
        RuleFor(x => x.AcceptedTerms)
            .Equal(true)
            .WithMessage("You must accept the terms and conditions to continue.")
            .WithAppErrorCode("TERMS_NOT_ACCEPTED");
    }
}