using FluentValidation;

namespace RateRelay.Application.Features.Queries.Accounts;

public class AccountsQueryValidator : AbstractValidator<AccountsQuery>
{
    public AccountsQueryValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty()
            .WithMessage("Account ID is required.")
            .Must(x => x > 0)
            .WithMessage("Account ID must be a positive number.");
    }
}