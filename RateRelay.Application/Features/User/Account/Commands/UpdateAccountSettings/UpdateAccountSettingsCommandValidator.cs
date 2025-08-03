using FluentValidation;

namespace RateRelay.Application.Features.User.Account.Commands.UpdateAccountSettings;

public class UpdateAccountSettingsCommandValidator : AbstractValidator<UpdateAccountSettingsCommand>
{
    public UpdateAccountSettingsCommandValidator()
    {
    }
}