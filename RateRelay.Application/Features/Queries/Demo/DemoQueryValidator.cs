using FluentValidation;
using RateRelay.Application.Extensions;

namespace RateRelay.Application.Features.Queries.Demo;

public class DemoQueryValidator : AbstractValidator<DemoQuery>
{
    public DemoQueryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .WithAppErrorCode("NAME_REQUIRED")
            .MaximumLength(50).WithMessage("Name must not exceed 50 characters.")
            .WithAppErrorCode("NAME_TOO_LONG");

        RuleFor(x => x.Age)
            .InclusiveBetween(1, 120).WithMessage("Age must be between 1 and 120.")
            .WithAppErrorCode("AGE_INVALID_RANGE");
    }
}