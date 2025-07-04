using RateRelay.Domain.Common.DTOs;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Enums;

namespace RateRelay.Domain.Interfaces;

public interface IReferralService
{
    Task<string> GenerateReferralCodeAsync(long accountId, CancellationToken cancellationToken = default);

    Task<AccountEntity?> GetAccountByReferralCodeAsync(string referralCode,
        CancellationToken cancellationToken = default);

    Task<bool> LinkReferralAsync(long referredAccountId, string referralCode,
        CancellationToken cancellationToken = default);

    Task UpdateReferralProgressAsync(long accountId, ReferralGoalType goalType, int incrementValue = 1,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<ReferralProgressEntity>> GetReferralProgressAsync(long referrerAccountId,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<ReferralRewardEntity>> GetReferralRewardsAsync(long accountId,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<ReferralGoalEntity>> GetActiveGoalsAsync(CancellationToken cancellationToken = default);

    Task<ReferralStatsDto> GetReferralStatsAsync(long accountId, CancellationToken cancellationToken = default);

    Task ProcessGoalCompletionAsync(long accountId, ReferralGoalType goalType, int currentValue,
        CancellationToken cancellationToken = default);
    
    Task<ReferralGoalEntity?> GetGoalByTypeAsync(ReferralGoalType goalType,
        CancellationToken cancellationToken = default);

    Task<int?> GetGoalRewardPointsByTypeAsync(ReferralGoalType goalType,
        CancellationToken cancellationToken = default);
}