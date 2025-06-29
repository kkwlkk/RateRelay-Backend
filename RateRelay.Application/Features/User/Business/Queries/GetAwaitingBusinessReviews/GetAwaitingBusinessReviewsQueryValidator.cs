using FluentValidation;

namespace RateRelay.Application.Features.Business.Queries.GetAwaitingBusinessReviews;

public class GetAwaitingBusinessReviewsQueryValidator : AbstractValidator<GetAwaitingBusinessReviewsQuery>
{
    public GetAwaitingBusinessReviewsQueryValidator()
    {
        RuleFor(x => x.BusinessId)
            .NotEmpty()
            .WithMessage("BusinessId must not be empty.")
            .GreaterThan(0)
            .WithMessage("BusinessId must be greater than 0.")
            .Must(x => long.TryParse(x.ToString(), out _))
            .WithMessage("BusinessId must be a valid integer.");
    }
}