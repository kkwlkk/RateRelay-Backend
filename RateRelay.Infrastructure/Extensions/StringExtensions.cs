using System.IdentityModel.Tokens.Jwt;

namespace RateRelay.Infrastructure.Extensions;

public static class StringExtensions
{
    public static bool IsValidJwtFormat(this string jwt)
    {
        if (string.IsNullOrWhiteSpace(jwt))
            return false;

        var parts = jwt.Split('.');
        if (parts.Length != 3)
            return false;
            
        try
        {
            var handler = new JwtSecurityTokenHandler();

            return handler.ReadToken(jwt) is JwtSecurityToken;
        }
        catch (Exception)
        {
            return false;
        }
    }
}