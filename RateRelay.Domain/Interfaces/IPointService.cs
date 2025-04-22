using RateRelay.Domain.Entities;
using RateRelay.Domain.Enums;

namespace RateRelay.Domain.Interfaces;

public interface IPointService
{
    Task<int> GetPointBalanceAsync(long accountId, CancellationToken cancellationToken = default);

    Task<bool> AddPointsAsync(long accountId, int amount,
        PointTransactionType transactionType = PointTransactionType.System,
        string? description = null,
        CancellationToken cancellationToken = default);

    Task<bool> DeductPointsAsync(long accountId, int amount,
        PointTransactionType transactionType = PointTransactionType.System,
        string? description = null,
        CancellationToken cancellationToken = default);

    Task<bool> HasEnoughPointsAsync(long accountId, int amount,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<PointTransactionEntity>> GetPointTransactionsAsync(long accountId,
        CancellationToken cancellationToken = default);
}