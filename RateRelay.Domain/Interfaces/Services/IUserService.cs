using RateRelay.Domain.Entities;

namespace RateRelay.Domain.Interfaces;

public interface IUserService
{
    Task<AccountEntity> GetByIdAsync(long accountId, CancellationToken cancellationToken = default);
    Task<bool> IsDisplayNameTakenAsync(string displayName, long? excludeAccountId = null, CancellationToken cancellationToken = default);
}