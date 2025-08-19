using RateRelay.Domain.Common;
using RateRelay.Domain.Entities;

namespace RateRelay.Domain.Interfaces;

public interface IAuthService
{
    Task<string> GenerateJwtTokenAsync(AccountEntity account);
    Task<string> GenerateRefreshTokenAsync(AccountEntity account);
    Task InvalidateRefreshTokenAsync(string refreshToken);
    Task<ulong> GetEffectivePermissionsAsync(long accountId);
    Task<GoogleUserInfo> ValidateGoogleTokenAsync(string token);
}