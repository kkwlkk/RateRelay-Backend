using System.Security.Claims;

namespace RateRelay.Infrastructure.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static long GetUserId(this ClaimsPrincipal claimsPrincipal)
    {
        var claim = claimsPrincipal.FindFirst("sub");
        if (claim is null)
        {
            throw new InvalidOperationException("User ID claim not found.");
        }

        return long.TryParse(claim.Value, out var userId) ? userId : 0;
    }

    public static string GetUserName(this ClaimsPrincipal claimsPrincipal)
    {
        var claim = claimsPrincipal.FindFirst("name");
        if (claim is null)
        {
            throw new InvalidOperationException("User name claim not found.");
        }

        return claim.Value;
    }

    public static T GetClaimValue<T>(this ClaimsPrincipal claimsPrincipal, string claimType, T defaultValue = default)
    {
        var claim = claimsPrincipal.FindFirst(claimType);
        
        if (claim == null)
        {
            return defaultValue;
        }

        try
        {
            if (typeof(T) == typeof(string))
            {
                return (T)(object)claim.Value;
            }
            
            return (T)Convert.ChangeType(claim.Value, typeof(T));
        }
        catch
        {
            return defaultValue;
        }
    }
}