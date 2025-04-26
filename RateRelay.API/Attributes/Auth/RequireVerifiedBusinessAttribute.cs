using Microsoft.AspNetCore.Authorization;

namespace RateRelay.API.Attributes.Auth;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequireVerifiedBusinessAttribute : AuthorizeAttribute
{
    public RequireVerifiedBusinessAttribute()
    {
        Policy = "RequireVerifiedBusiness";
    }
}