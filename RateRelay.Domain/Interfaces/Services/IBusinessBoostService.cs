using RateRelay.Domain.Common.DTOs;

namespace RateRelay.Domain.Interfaces;

public interface IBusinessBoostService
{
    Task<BusinessBoostResultDto> BoostBusinessAsync(long businessId, long? changedByAccountId, byte newPriority, int targetReviews, string reason, CancellationToken cancellationToken = default);
    Task<BusinessBoostResultDto> UnboostBusinessAsync(long businessId, long? changedByAccountId, string reason, CancellationToken cancellationToken = default);
    Task<bool> IsBusinessBoostedAsync(long businessId, CancellationToken cancellationToken = default);
}