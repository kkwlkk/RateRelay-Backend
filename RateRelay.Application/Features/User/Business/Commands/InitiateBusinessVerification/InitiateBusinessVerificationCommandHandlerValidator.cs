using FluentValidation;

namespace RateRelay.Application.Features.Business.Commands.InitiateBusinessVerification;

public class InitiateBusinessVerificationCommandHandlerValidator : AbstractValidator<InitiateBusinessVerificationCommand>
{
    public InitiateBusinessVerificationCommandHandlerValidator()
    {
        RuleFor(x => x.PlaceId)
            .NotEmpty()
            .WithMessage("PlaceId is required.");
    }
}