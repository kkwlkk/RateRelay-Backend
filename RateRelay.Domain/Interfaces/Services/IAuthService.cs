using RateRelay.Domain.Common;
using RateRelay.Domain.Entities;

namespace RateRelay.Domain.Interfaces;

public interface IAuthService
{
    Task<string> GenerateJwtTokenAsync(AccountEntity account);
    Task<string> GenerateRefreshTokenAsync(AccountEntity account);
    Task InvalidateRefreshTokenAsync(string refreshToken);
    bool VerifyPassword(string passwordHash, string password);
    string HashPassword(string password);
    Task<ulong> GetEffectivePermissionsAsync(long accountId);
    Task<GoogleUserInfo> ValidateGoogleTokenAsync(string token);
}