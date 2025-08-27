using MediatR;
using Microsoft.AspNetCore.Mvc;
using RateRelay.Domain.Enums;

namespace RateRelay.Application.Features.User.Account.Commands.UpdateAccountSettings;

public class UpdateAccountSettingsCommand : IRequest<IActionResult>
{
    public EmailPreferencesFlags? EmailPreferences { get; set; }
}