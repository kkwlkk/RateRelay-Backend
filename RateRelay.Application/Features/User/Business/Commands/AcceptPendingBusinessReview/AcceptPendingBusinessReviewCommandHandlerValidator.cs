using FluentValidation;
using RateRelay.Application.Features.User.Business.Commands.AcceptPendingBusinessReview;

namespace RateRelay.Application.Features.Business.Commands.AcceptPendingBusinessReview;

public class AcceptPendingBusinessReviewCommandHandlerValidator : AbstractValidator<AcceptPendingBusinessReviewCommand>
{
    public AcceptPendingBusinessReviewCommandHandlerValidator()
    {
        RuleFor(x => x.ReviewId)
            .NotEmpty()
            .WithMessage("Review ID is required.")
            .GreaterThan(0)
            .WithMessage("Review ID must be greater than 0.")
            .Must(x => long.TryParse(x.ToString(), out _))
            .WithMessage("Review ID must be a valid integer.");
    }
}