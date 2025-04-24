using FluentValidation;

namespace RateRelay.Application.Features.Business.Commands.RejectPendingBusinessReview;

public class RejectPendingBusinessReviewCommandHandlerValidator : AbstractValidator<RejectPendingBusinessReviewCommand>
{
    public RejectPendingBusinessReviewCommandHandlerValidator()
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