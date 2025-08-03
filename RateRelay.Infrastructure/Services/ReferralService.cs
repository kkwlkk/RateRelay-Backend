using Microsoft.EntityFrameworkCore;
using RateRelay.Domain.Common.DTOs;
using RateRelay.Domain.Constants;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Enums;
using RateRelay.Domain.Exceptions;
using RateRelay.Domain.Interfaces;
using RateRelay.Domain.Interfaces.DataAccess;
using RateRelay.Infrastructure.Helpers;
using Serilog;

namespace RateRelay.Infrastructure.Services;

public class ReferralService(
    IUnitOfWorkFactory unitOfWorkFactory,
    IPointService pointService,
    ILogger logger
) : IReferralService
{
    private const int ReferralCodeLength = 8;
    private const string ReferralCodeChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public async Task<string> GenerateReferralCodeAsync(long accountId, CancellationToken cancellationToken = default)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var accountRepository = unitOfWork.GetRepository<AccountEntity>();

        var account = await accountRepository.GetByIdAsync(accountId, cancellationToken);
        if (account is null)
        {
            throw new NotFoundException($"Account with ID {accountId} not found.");
        }

        if (!string.IsNullOrEmpty(account.ReferralCode))
        {
            return account.ReferralCode;
        }

        string referralCode;
        bool isUnique;
        var attempts = 0;
        const int maxAttempts = 10;

        do
        {
            referralCode = RandomHelper.GetRandomString(ReferralCodeLength, ReferralCodeChars);
            isUnique = !await accountRepository.GetBaseQueryable()
                .AnyAsync(a => a.ReferralCode == referralCode, cancellationToken);
            attempts++;
        } while (!isUnique && attempts < maxAttempts);

        if (!isUnique)
        {
            throw new AppException("Unable to generate unique referral code. Please try again.");
        }

        accountRepository.Update(account);
        account.ReferralCode = referralCode;
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.Information("Generated referral code {ReferralCode} for account {AccountId}",
            referralCode, accountId);

        return referralCode;
    }

    public async Task<AccountEntity?> GetAccountByReferralCodeAsync(string referralCode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(referralCode))
        {
            return null;
        }

        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var accountRepository = unitOfWork.GetRepository<AccountEntity>();

        return await accountRepository.GetBaseQueryable()
            .FirstOrDefaultAsync(a => a.ReferralCode == referralCode.ToUpper(), cancellationToken);
    }

    public async Task<bool> LinkReferralAsync(long referredAccountId, string referralCode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(referralCode))
        {
            return false;
        }

        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var accountRepository = unitOfWork.GetRepository<AccountEntity>();

        var referredAccount = await accountRepository.GetByIdAsync(referredAccountId, cancellationToken);
        if (referredAccount is null)
        {
            throw new NotFoundException($"Referred account with ID {referredAccountId} not found.");
        }

        if (referredAccount.ReferredByAccountId.HasValue)
        {
            logger.Warning("Account {AccountId} already has a referrer", referredAccountId);
            return false;
        }

        var referrerAccount = await GetAccountByReferralCodeAsync(referralCode, cancellationToken);
        if (referrerAccount is null)
        {
            logger.Warning("Invalid referral code {ReferralCode}", referralCode);
            return false;
        }

        if (referrerAccount.Id == referredAccountId)
        {
            logger.Warning("Account {AccountId} attempted to refer themselves", referredAccountId);
            return false;
        }

        try
        {
            await unitOfWork.BeginTransactionAsync(cancellationToken: cancellationToken);

            accountRepository.Update(referredAccount);
            referredAccount.ReferredByAccountId = referrerAccount.Id;

            await InitializeReferralProgressAsync(unitOfWork, referrerAccount.Id, referredAccountId, cancellationToken);

            await pointService.AddPointsAsync(
                referredAccountId,
                PointConstants.ReferralWelcomeBonusPoints,
                PointTransactionType.ReferralWelcomeBonus,
                $"Welcome bonus for being referred by {referrerAccount.Id}",
                cancellationToken
            );

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            logger.Information("Successfully linked referral: Referrer {ReferrerId} -> Referred {ReferredId}",
                referrerAccount.Id, referredAccountId);

            return true;
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.Error(ex, "Failed to link referral for account {AccountId} with code {ReferralCode}",
                referredAccountId, referralCode);
            throw;
        }
    }

    public async Task UpdateReferralProgressAsync(long accountId, ReferralGoalType goalType,
        int incrementValue = 1, CancellationToken cancellationToken = default)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var accountRepository = unitOfWork.GetRepository<AccountEntity>();

        var account = await accountRepository.GetBaseQueryable()
            .Include(a => a.ReferredBy)
            .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);

        if (account?.ReferredByAccountId is null)
        {
            return;
        }

        var progressRepository = unitOfWork.GetRepository<ReferralProgressEntity>();
        var goalRepository = unitOfWork.GetRepository<ReferralGoalEntity>();

        var activeGoals = await goalRepository.GetBaseQueryable()
            .Where(g => g.GoalType == goalType && g.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var goal in activeGoals)
        {
            var progress = await progressRepository.GetBaseQueryable()
                .Include(p => p.Goal)
                .FirstOrDefaultAsync(p =>
                    p.ReferrerAccountId == account.ReferredByAccountId &&
                    p.ReferredAccountId == accountId &&
                    p.GoalId == goal.Id, cancellationToken);

            if (progress is null)
            {
                progress = new ReferralProgressEntity
                {
                    ReferrerAccountId = account.ReferredByAccountId.Value,
                    ReferredAccountId = accountId,
                    GoalId = goal.Id,
                    CurrentValue = 0
                };
                await progressRepository.InsertAsync(progress, cancellationToken);
            }

            var previousValue = progress.CurrentValue;
            progress.CurrentValue += incrementValue;

            var wasCompleted = progress.IsCompleted;
            progress.UpdateProgress(progress.CurrentValue);

            if (!wasCompleted && progress.IsCompleted)
            {
                await ProcessRewardAsync(unitOfWork, progress, cancellationToken);

                logger.Information(
                    "Referral goal completed: Goal {GoalId} for Referrer {ReferrerId} -> Referred {ReferredId}",
                    goal.Id, account.ReferredByAccountId, accountId);
            }

            progressRepository.Update(progress);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<ReferralProgressEntity>> GetReferralProgressAsync(long referrerAccountId,
        CancellationToken cancellationToken = default)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var progressRepository = unitOfWork.GetRepository<ReferralProgressEntity>();

        return await progressRepository.GetBaseQueryable()
            .Include(p => p.Goal)
            .Include(p => p.Referred)
            .Where(p => p.ReferrerAccountId == referrerAccountId)
            .OrderBy(p => p.Goal.SortOrder)
            .ThenByDescending(p => p.DateCreatedUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ReferralRewardEntity>> GetReferralRewardsAsync(long accountId,
        CancellationToken cancellationToken = default)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var rewardRepository = unitOfWork.GetRepository<ReferralRewardEntity>();

        return await rewardRepository.GetBaseQueryable()
            .Include(r => r.Goal)
            .Include(r => r.Referred)
            .Where(r => r.ReferrerAccountId == accountId)
            .OrderByDescending(r => r.DateAwardedUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ReferralGoalEntity>> GetActiveGoalsAsync(
        CancellationToken cancellationToken = default)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var goalRepository = unitOfWork.GetRepository<ReferralGoalEntity>();

        return await goalRepository.GetBaseQueryable()
            .Where(g => g.IsActive)
            .OrderBy(g => g.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<ReferralStatsDto> GetReferralStatsAsync(long accountId,
        CancellationToken cancellationToken = default)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var accountRepository = unitOfWork.GetRepository<AccountEntity>();
        var progressRepository = unitOfWork.GetRepository<ReferralProgressEntity>();
        var rewardRepository = unitOfWork.GetRepository<ReferralRewardEntity>();

        var account = await accountRepository.GetBaseQueryable()
            .Include(a => a.ReferredBy)
            .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);

        if (account is null)
        {
            throw new NotFoundException($"Account with ID {accountId} not found.");
        }

        var totalReferrals = await accountRepository.GetBaseQueryable()
            .CountAsync(a => a.ReferredByAccountId == accountId, cancellationToken);

        var activeReferrals = await accountRepository.GetBaseQueryable()
            .Where(a => a.ReferredByAccountId == accountId &&
                        a.OnboardingStep == AccountOnboardingStep.Completed)
            .CountAsync(cancellationToken);

        var completedGoals = await progressRepository.GetBaseQueryable()
            .CountAsync(p => p.ReferrerAccountId == accountId && p.IsCompleted, cancellationToken);

        var totalPointsEarned = await rewardRepository.GetBaseQueryable()
            .Where(r => r.ReferrerAccountId == accountId)
            .SumAsync(r => r.RewardPoints, cancellationToken);

        var progressSummaries = await progressRepository.GetBaseQueryable()
            .Include(p => p.Goal)
            .Include(p => p.Referred)
            .Where(p => p.ReferrerAccountId == accountId)
            .Select(p => new ReferralProgressSummary
            {
                GoalName = p.Goal.Name,
                GoalDescription = p.Goal.Description,
                TargetValue = p.Goal.TargetValue,
                CurrentValue = p.CurrentValue,
                RewardPoints = p.Goal.RewardPoints,
                IsCompleted = p.IsCompleted,
                DateCompleted = p.DateCompletedUtc,
                ReferredUserName = p.Referred.GoogleUsername
            })
            .ToListAsync(cancellationToken);

        var pendingRewards = progressSummaries
            .Where(p => p is { IsCompleted: false, CurrentValue: > 0 })
            .Sum(p => p.RewardPoints);

        return new ReferralStatsDto
        {
            TotalReferrals = totalReferrals,
            ActiveReferrals = activeReferrals,
            CompletedGoals = completedGoals,
            TotalPointsEarned = totalPointsEarned,
            PendingRewards = pendingRewards,
            ReferralCode = account.ReferralCode ?? await GenerateReferralCodeAsync(accountId, cancellationToken),
            ReferredByCode = account.ReferredBy?.ReferralCode,
            Progress = progressSummaries
        };
    }

    public async Task ProcessGoalCompletionAsync(long accountId, ReferralGoalType goalType, int currentValue,
        CancellationToken cancellationToken = default)
    {
        await UpdateReferralProgressAsync(accountId, goalType, 0, cancellationToken);
    }

    public async Task<ReferralGoalEntity?> GetGoalByTypeAsync(ReferralGoalType goalType,
        CancellationToken cancellationToken = default)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var goalRepository = unitOfWork.GetRepository<ReferralGoalEntity>();

        return await goalRepository.GetBaseQueryable()
            .FirstOrDefaultAsync(g => g.GoalType == goalType && g.IsActive, cancellationToken);
    }

    public async Task<int?> GetGoalRewardPointsByTypeAsync(ReferralGoalType goalType,
        CancellationToken cancellationToken = default)
    {
        var goal = await GetGoalByTypeAsync(goalType, cancellationToken);
        return goal?.RewardPoints;
    }

    private async Task InitializeReferralProgressAsync(IUnitOfWork unitOfWork, long referrerAccountId,
        long referredAccountId, CancellationToken cancellationToken)
    {
        var goalRepository = unitOfWork.GetRepository<ReferralGoalEntity>();
        var progressRepository = unitOfWork.GetRepository<ReferralProgressEntity>();

        var activeGoals = await goalRepository.GetBaseQueryable()
            .Where(g => g.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var goal in activeGoals)
        {
            var progress = new ReferralProgressEntity
            {
                ReferrerAccountId = referrerAccountId,
                ReferredAccountId = referredAccountId,
                GoalId = goal.Id,
                CurrentValue = 0
            };

            await progressRepository.InsertAsync(progress, cancellationToken);
        }
    }

    private async Task ProcessRewardAsync(IUnitOfWork unitOfWork, ReferralProgressEntity progress,
        CancellationToken cancellationToken)
    {
        var rewardRepository = unitOfWork.GetRepository<ReferralRewardEntity>();

        var existingReward = await rewardRepository.GetBaseQueryable()
            .FirstOrDefaultAsync(r =>
                r.ReferrerAccountId == progress.ReferrerAccountId &&
                r.ReferredAccountId == progress.ReferredAccountId &&
                r.GoalId == progress.GoalId, cancellationToken);

        if (existingReward is not null)
        {
            logger.Warning("Reward already exists for referral progress {ProgressId}", progress.Id);
            return;
        }

        var reward = new ReferralRewardEntity
        {
            ReferrerAccountId = progress.ReferrerAccountId,
            ReferredAccountId = progress.ReferredAccountId,
            GoalId = progress.GoalId,
            RewardPoints = progress.Goal.RewardPoints,
            DateAwardedUtc = DateTime.UtcNow
        };

        await rewardRepository.InsertAsync(reward, cancellationToken);

        await pointService.AddPointsAsync(
            progress.ReferrerAccountId,
            progress.Goal.RewardPoints,
            PointTransactionType.ReferralReward,
            $"Referral reward for {progress.Goal.Name} completed by referred user",
            cancellationToken
        );

        logger.Information("Awarded {Points} points to referrer {ReferrerId} for goal {GoalName}",
            progress.Goal.RewardPoints, progress.ReferrerAccountId, progress.Goal.Name);
    }
}