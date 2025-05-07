using FluentValidation;

namespace RateRelay.Application.Features.ReviewableBusiness.Commands.SubmitBusinessReview;

public class SubmitBusinessReviewCommandValidator : AbstractValidator<SubmitBusinessReviewCommand>
{
    public SubmitBusinessReviewCommandValidator()
    {
        RuleFor(x => x.Rating)
            .IsInEnum()
            .WithMessage("Invalid rating value. Allowed values are: 1, 2, 3, 4, 5.");

        RuleFor(x => x.Comment)
            .NotEmpty()
            .WithMessage("Comment cannot be empty.")
            .MinimumLength(10)
            .WithMessage("Comment must be at least 10 characters long.")
            .MaximumLength(512)
            .WithMessage("Comment cannot exceed 512 characters.");

        RuleFor(x => x.PostedGoogleReview)
            .NotNull()
            .WithMessage("PostedGoogleReview cannot be null.")
            .Must(x => x is bool)
            .WithMessage("PostedGoogleReview must be a boolean value.");
    }
}