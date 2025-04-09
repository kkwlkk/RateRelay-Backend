using FluentValidation;
using RateRelay.Application.Extensions;

namespace RateRelay.Application.Features.Accounts.Queries;

public class AccountsQueryValidator : AbstractValidator<AccountsQuery>
{
    public AccountsQueryValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty()
            .WithMessage("Account ID is required.")
            .WithAppErrorCode("ACCOUNT_ID_REQUIRED")
            .Must(x => x > 0)
            .WithMessage("Account ID must be a positive number.")
            .WithAppErrorCode("ACCOUNT_ID_POSITIVE");
    }
}