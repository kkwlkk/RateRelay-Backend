using RateRelay.Domain.Entities;

namespace RateRelay.Domain.Interfaces;

public interface IBusinessQueueService
{
    Task<BusinessEntity?> GetNextAvailableBusinessForUserAsync(long accountId,
        int maxAttempts = 10,
        CancellationToken cancellationToken = default);

    Task<bool> IsBusinessInUseAsync(long businessId, CancellationToken cancellationToken = default);
    Task<BusinessEntity?> GetUserAssignedBusinessAsync(long accountId, CancellationToken cancellationToken = default);

    Task<bool> IsBusinessAssignedToUserAsync(long businessId, long accountId,
        CancellationToken cancellationToken = default);

    Task<TimeSpan?> GetAssignedBusinessLockTtlByUserAsync(long accountId, CancellationToken cancellationToken = default);
}