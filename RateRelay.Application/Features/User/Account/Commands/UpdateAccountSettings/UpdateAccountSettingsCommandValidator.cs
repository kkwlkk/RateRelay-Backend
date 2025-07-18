using FluentValidation;

namespace RateRelay.Application.Features.User.Account.Commands.UpdateAccountSettings;

public class UpdateAccountSettingsCommandValidator : AbstractValidator<UpdateAccountSettingsCommand>
{
    public UpdateAccountSettingsCommandValidator()
    {
        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .WithMessage("Display name cannot be empty.")
            .MaximumLength(64)
            .WithMessage("Display name cannot exceed 64 characters.");
    }
}