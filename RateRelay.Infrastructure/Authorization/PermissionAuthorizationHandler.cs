using Microsoft.AspNetCore.Authorization;
using RateRelay.Domain.Enums;
using RateRelay.Domain.Extensions;
using RateRelay.Domain.Extensions.Account;

namespace RateRelay.Infrastructure.Authorization;

public class PermissionRequirement(Permission permission) : IAuthorizationRequirement
{
    public Permission Permission { get; } = permission;
}

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (!context.User.HasClaim(c => c.Type == "permissions"))
        {
            return Task.CompletedTask;
        }

        var permissionsClaim = context.User.FindFirst("permissions");
        if (permissionsClaim == null || !ulong.TryParse(permissionsClaim.Value, out var permissions))
        {
            return Task.CompletedTask;
        }

        if (permissions.HasPermission(requirement.Permission))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}