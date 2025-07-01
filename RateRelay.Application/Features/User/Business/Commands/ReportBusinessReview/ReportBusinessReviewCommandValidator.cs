using FluentValidation;
using RateRelay.Application.Extensions;

namespace RateRelay.Application.Features.User.Business.Commands.ReportBusinessReview;

public class ReportBusinessReviewCommandValidator : AbstractValidator<ReportBusinessReviewCommand>
{
    public ReportBusinessReviewCommandValidator()
    {
        RuleFor(x => x.ReviewId)
            .GreaterThan(0)
            .WithMessage("Review ID must be greater than 0.")
            .WithAppErrorCode("InvalidReviewId");
        
        RuleFor(x => x.BusinessId)
            .GreaterThan(0)
            .WithMessage("Business ID must be greater than 0.")
            .WithAppErrorCode("InvalidBusinessId");
        
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title cannot be empty.")
            .WithAppErrorCode("TitleEmpty")
            .MaximumLength(64)
            .WithMessage("Title cannot exceed 64 characters.")
            .WithErrorCode("TitleTooLong");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Content cannot be empty.")
            .WithAppErrorCode("ContentEmpty")
            .MaximumLength(500)
            .WithMessage("Content cannot exceed 500 characters.")
            .WithErrorCode("ContentTooLong");

        RuleFor(x => x.Reason)
            .IsInEnum()
            .WithMessage("Invalid report reason specified.")
            .WithAppErrorCode("InvalidReportReason");
    }
}