using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace RateRelay.Application.Features.User.Account.Commands.UpdateAccountSettings;

public class UpdateAccountSettingsCommand : IRequest<IActionResult>
{
    public string? DisplayName { get; set; }
}