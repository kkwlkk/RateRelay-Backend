using RateRelay.Domain.Entities;

namespace RateRelay.Domain.Interfaces;

public interface IUserService
{
    Task<AccountEntity> GetByIdAsync(long accountId, CancellationToken cancellationToken = default);
    Task<AccountBanEntity?> HasActiveBanAsync(long accountId, CancellationToken cancellationToken = default);
    Task BanAccountAsync(long accountId, string reason, DateTime? expiresAtUtc = null, CancellationToken cancellationToken = default);
}