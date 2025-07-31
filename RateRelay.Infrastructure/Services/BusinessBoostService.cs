using Microsoft.EntityFrameworkCore;
using RateRelay.Domain.Common.DTOs;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Enums;
using RateRelay.Domain.Exceptions;
using RateRelay.Domain.Interfaces;
using RateRelay.Domain.Interfaces.DataAccess;

namespace RateRelay.Infrastructure.Services;

public class BusinessBoostService(
    IUnitOfWorkFactory unitOfWorkFactory
) : IBusinessBoostService
{
    public async Task<BusinessBoostResultDto> BoostBusinessAsync(long businessId, long? changedByAccountId, byte newPriority, int targetReviews,
        string reason, CancellationToken cancellationToken = default)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var businessRepository = unitOfWork.GetRepository<BusinessEntity>();
        var reviewRepository = unitOfWork.GetRepository<BusinessReviewEntity>();
        var historyRepository = unitOfWork.GetRepository<BusinessPriorityHistoryEntity>();
        var boostRepository = unitOfWork.GetRepository<BusinessBoostEntity>();

        var business = await businessRepository.GetByIdAsync(businessId, cancellationToken);
        if (business is null)
        {
            throw new NotFoundException($"Business with ID {businessId} not found.");
        }

        var existingBoost = await boostRepository.GetBaseQueryable()
            .Where(b => b.BusinessId == businessId && b.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingBoost is not null)
        {
            throw new AppException($"Business with ID {businessId} is already boosted.");
        }

        var currentReviews = await reviewRepository.CountAsync(
            r => r.BusinessId == businessId && r.Status == BusinessReviewStatus.Accepted,
            cancellationToken);

        var oldPriority = business.Priority;
        business.Priority = newPriority;
        businessRepository.Update(business);

        var historyEntry = new BusinessPriorityHistoryEntity
        {
            BusinessId = businessId,
            OldPriority = oldPriority,
            NewPriority = newPriority,
            ChangedById = changedByAccountId,
            ChangedBySystem = changedByAccountId is null,
            Reason = reason
        };

        var boost = new BusinessBoostEntity
        {
            BusinessId = businessId,
            TargetReviews = targetReviews,
            CreatedById = changedByAccountId,
            CreatedBySystem = changedByAccountId is null,
            BoostedAt = DateTime.UtcNow,
            IsActive = true
        };

        await historyRepository.InsertAsync(historyEntry, cancellationToken);
        await boostRepository.InsertAsync(boost, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new BusinessBoostResultDto
        {
            BusinessId = businessId,
            BusinessName = business.BusinessName,
            OldPriority = oldPriority,
            NewPriority = newPriority,
            CurrentReviews = currentReviews,
            ReviewsNeededForTarget = Math.Max(0, targetReviews - currentReviews),
            Reason = reason,
            BoostedAt = boost.BoostedAt,
            IsNowBoosted = true
        };
    }

    public async Task<BusinessBoostResultDto> UnboostBusinessAsync(long businessId, long? changedByAccountId, string reason,
        CancellationToken cancellationToken = default)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var businessRepository = unitOfWork.GetRepository<BusinessEntity>();
        var reviewRepository = unitOfWork.GetRepository<BusinessReviewEntity>();
        var historyRepository = unitOfWork.GetRepository<BusinessPriorityHistoryEntity>();
        var boostRepository = unitOfWork.GetRepository<BusinessBoostEntity>();

        var business = await businessRepository.GetByIdAsync(businessId, cancellationToken);
        if (business is null)
        {
            throw new NotFoundException($"Business with ID {businessId} not found.");
        }

        var activeBoost = await boostRepository.GetBaseQueryable()
            .Where(b => b.BusinessId == businessId && b.IsActive)
            .OrderByDescending(b => b.BoostedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (activeBoost is null)
        {
            throw new AppException($"Business with ID {businessId} is not currently boosted.");
        }

        var latestHistoryEntry = await historyRepository.GetBaseQueryable()
            .Where(h => h.BusinessId == businessId)
            .OrderByDescending(h => h.DateCreatedUtc)
            .FirstOrDefaultAsync(cancellationToken);

        var currentReviews = await reviewRepository.CountAsync(
            r => r.BusinessId == businessId && r.Status == BusinessReviewStatus.Accepted,
            cancellationToken);

        var oldPriority = business.Priority;
        var restorePriority = byte.MinValue;

        if (latestHistoryEntry != null && latestHistoryEntry.NewPriority == oldPriority)
        {
            restorePriority = latestHistoryEntry.OldPriority;
        }

        business.Priority = restorePriority;
        businessRepository.Update(business);

        var historyEntry = new BusinessPriorityHistoryEntity
        {
            BusinessId = businessId,
            OldPriority = oldPriority,
            NewPriority = restorePriority,
            ChangedById = changedByAccountId,
            ChangedBySystem = changedByAccountId is null,
            Reason = reason
        };

        activeBoost.IsActive = false;
        activeBoost.CompletedAt = DateTime.UtcNow;
        boostRepository.Update(activeBoost);

        await historyRepository.InsertAsync(historyEntry, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new BusinessBoostResultDto
        {
            BusinessId = businessId,
            BusinessName = business.BusinessName,   
            OldPriority = oldPriority,
            NewPriority = restorePriority,
            CurrentReviews = currentReviews,
            ReviewsNeededForTarget = Math.Max(0, activeBoost.TargetReviews - currentReviews),
            Reason = reason,
            BoostedAt = activeBoost.BoostedAt,
            IsNowBoosted = false
        };
    }

    public async Task<bool> IsBusinessBoostedAsync(long businessId, CancellationToken cancellationToken = default)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var boostRepository = unitOfWork.GetRepository<BusinessBoostEntity>();

        return await boostRepository.GetBaseQueryable()
            .Where(b => b.BusinessId == businessId && b.IsActive)
            .AnyAsync(cancellationToken);
    }
}