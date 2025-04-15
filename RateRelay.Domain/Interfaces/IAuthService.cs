using RateRelay.Domain.Common;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Enums;

namespace RateRelay.Domain.Interfaces;

public interface IAuthService
{
    Task<string> GenerateJwtTokenAsync(AccountEntity account);
    Task<string> GenerateRefreshTokenAsync(AccountEntity account);
    Task InvalidateRefreshTokenAsync(string refreshToken);
    bool VerifyPassword(string passwordHash, string password);
    string HashPassword(string password);
    bool HasPermission(ulong permissions, Permission permission);
    Task<ulong> GetEffectivePermissionsAsync(long accountId);
    Task<GoogleUserInfo> ValidateGoogleTokenAsync(string token);
}