using System.Security.Claims;

namespace RateRelay.Infrastructure.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static long GetUserId(this ClaimsPrincipal claimsPrincipal)
    {
        var claim = claimsPrincipal.FindFirst("sub");
        if (claim is null)
        {
            throw new ArgumentNullException(nameof(claim), "User ID claim not found.");
        }

        return long.Parse(claim.Value);
    }

    public static string GetUserName(this ClaimsPrincipal claimsPrincipal)
    {
        var claim = claimsPrincipal.FindFirst("name");
        if (claim is null)
        {
            throw new ArgumentNullException(nameof(claim), "User name claim not found.");
        }

        return claim.Value;
    }
}