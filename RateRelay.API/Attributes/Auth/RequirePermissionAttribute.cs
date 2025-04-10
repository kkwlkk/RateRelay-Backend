using Microsoft.AspNetCore.Authorization;
using RateRelay.Domain.Enums;

namespace RateRelay.API.Attributes.Auth;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequirePermissionAttribute : AuthorizeAttribute
{
    public RequirePermissionAttribute(Permission permission)
    {
        Policy = $"Permission:{permission}";
    }
}