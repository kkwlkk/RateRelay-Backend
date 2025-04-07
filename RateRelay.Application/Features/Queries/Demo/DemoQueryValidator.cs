using FluentValidation;

namespace RateRelay.Application.Features.Queries.Demo;

public class DemoQueryValidator : AbstractValidator<DemoQuery>
{
    public DemoQueryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .MaximumLength(50)
            .WithMessage("Name must not exceed 50 characters.");

        RuleFor(x => x.Age)
            .InclusiveBetween(1, 120)
            .WithMessage("Age must be between 1 and 120.");
    }
}