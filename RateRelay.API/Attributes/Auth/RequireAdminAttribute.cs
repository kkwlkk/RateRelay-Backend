using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RateRelay.Domain.Enums;
using RateRelay.Domain.Interfaces;

namespace RateRelay.API.Attributes.Auth;

public class RequireAdminAttribute : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var currentUserDataResolver = context.HttpContext.RequestServices
            .GetRequiredService<ICurrentUserDataResolver>();

        var userId = currentUserDataResolver.GetAccountId();

        if (userId == null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var userPermissions = currentUserDataResolver.GetPermissions();

        if (!HasAnyAdminPermission((Permission)userPermissions))
        {
            context.Result = new ForbidResult();
        }
    }

    private static bool HasAnyAdminPermission(Permission userPermissions)
    {
        return userPermissions != Permission.None;
    }
}