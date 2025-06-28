using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using BCrypt.Net;
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RateRelay.Domain.Common;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Enums;
using RateRelay.Domain.Extensions;
using RateRelay.Domain.Interfaces;
using RateRelay.Infrastructure.Configuration;
using RateRelay.Infrastructure.DataAccess.Context;
using Serilog;

namespace RateRelay.Infrastructure.Services;

public class AuthService(
    IOptions<JwtOptions> jwtAuthOptions,
    IOptions<GoogleOAuthOptions> googleAuthOptions,
    RateRelayDbContext dbContext)
    : IAuthService
{
    private readonly JwtOptions _jwtAuthOptions = jwtAuthOptions.Value;

    public async Task<string> GenerateJwtTokenAsync(AccountEntity account)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Convert.FromBase64String(_jwtAuthOptions.Secret);

        if (key.Length < 32)
        {
            throw new ArgumentException("Secret Key length must be at least 32 bytes.");
        }

        var securityKey = new SymmetricSecurityKey(key);

        var effectivePermissions = await GetEffectivePermissionsAsync(account.Id);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
            new(JwtRegisteredClaimNames.Name, account.Username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("permissions", effectivePermissions.ToString())
        };

        if (account.Role != null)
        {
            claims.Add(new Claim(ClaimTypes.Role, account.Role.Name));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(_jwtAuthOptions.Expiration),
            Issuer = _jwtAuthOptions.Issuer,
            Audience = _jwtAuthOptions.Audience,
            SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature),
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task<string> GenerateRefreshTokenAsync(AccountEntity account)
    {
        var refreshToken = GenerateRefreshToken();

        var refreshTokenEntity = new RefreshTokenEntity
        {
            AccountId = account.Id,
            Token = refreshToken,
            ExpirationDate = DateTime.UtcNow.Add(_jwtAuthOptions.RefreshExpiration),
        };

        await dbContext.Set<RefreshTokenEntity>().AddAsync(refreshTokenEntity);
        await dbContext.SaveChangesAsync();

        return refreshToken;
    }

    public async Task InvalidateRefreshTokenAsync(string refreshToken)
    {
        var tokenEntity = await dbContext.Set<RefreshTokenEntity>()
            .FirstOrDefaultAsync(t => t.Token == refreshToken);

        if (tokenEntity is not null)
        {
            dbContext.Set<RefreshTokenEntity>().Remove(tokenEntity);
            await dbContext.SaveChangesAsync();
        }
    }

    public bool VerifyPassword(string passwordHash, string password)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch (SaltParseException ex)
        {
            Log.Error(ex, "Password hash verification failed due to invalid hash format.");
            return false;
        }
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
    }

    public async Task<ulong> GetEffectivePermissionsAsync(long accountId)
    {
        var account = await dbContext.Set<AccountEntity>()
            .Include(a => a.Role)
            .FirstOrDefaultAsync(a => a.Id == accountId);

        if (account is null)
            return 0;

        var effectivePermissions = account.Permissions;

        if (account.Role is not null)
        {
            effectivePermissions |= account.Role.Permissions;
        }

        return effectivePermissions;
    }

    private string GenerateRefreshToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public async Task<GoogleUserInfo> ValidateGoogleTokenAsync(string token)
    {
        var clientId = googleAuthOptions.Value.ClientId;
        var validationSettings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = [clientId]
        };

        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(token, validationSettings);
            return new GoogleUserInfo
            {
                Email = payload.Email,
                Name = payload.Name,
                PictureUrl = payload.Picture,
                GoogleId = payload.Subject,
            };
        }
        catch (InvalidJwtException ex)
        {
            Log.Error(ex, "Invalid OAuth token.");
            throw new UnauthorizedAccessException("Invalid OAuth token.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error validating OAuth token.");
            throw;
        }
    }
}