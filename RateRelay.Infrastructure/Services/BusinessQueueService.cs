using Microsoft.EntityFrameworkCore;
using RateRelay.Domain.Constants;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Enums;
using RateRelay.Domain.Enums.Redis;
using RateRelay.Domain.Interfaces;
using RateRelay.Domain.Interfaces.DataAccess;
using RateRelay.Domain.Interfaces.DataAccess.Redis;
using RateRelay.Infrastructure.Environment;
using Serilog;

namespace RateRelay.Infrastructure.Services;

public class BusinessQueueService(
    IUnitOfWorkFactory unitOfWorkFactory,
    IRedisDistributedLockProvider distributedLockProvider,
    IRedisCacheProvider redisCacheProvider,
    ILogger logger
) : IBusinessQueueService
{
    public async Task<BusinessEntity?> GetNextAvailableBusinessForUserAsync(
        long accountId,
        int maxAttempts = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
            var businessRepository = unitOfWork.GetRepository<BusinessEntity>();
            var businessReviewRepository = unitOfWork.GetRepository<BusinessReviewEntity>();

            var existingAssignment = await GetUserAssignedBusinessAsync(accountId, cancellationToken);

            if (existingAssignment is not null)
            {
                if (existingAssignment.OwnerAccount != null && existingAssignment.OwnerAccount.Id == accountId)
                {
                    if (ApplicationEnvironment.Current().IsDevelopment)
                    {
                        logger.Debug(
                            "User {AccountId} was assigned to their own business {BusinessId}. Finding a new business.",
                            accountId, existingAssignment.Id);
                    }

                    await UnassignBusinessFromUserAsync(existingAssignment.Id, accountId);
                }
                else
                {
                    var hasReview = await businessReviewRepository
                        .GetBaseQueryable()
                        .AnyAsync(r => r.ReviewerId == accountId &&
                                       r.BusinessId == existingAssignment.Id &&
                                       (r.Status == BusinessReviewStatus.Pending ||
                                        r.Status == BusinessReviewStatus.Accepted),
                            cancellationToken);

                    if (hasReview)
                    {
                        logger.Information(
                            "User {AccountId} was assigned to business {BusinessId} they already reviewed. Finding a new business.",
                            accountId, existingAssignment.Id);

                        await UnassignBusinessFromUserAsync(existingAssignment.Id, accountId);
                    }
                    else
                    {
                        if (ApplicationEnvironment.Current().IsDevelopment)
                        {
                            logger.Debug("User {AccountId} already has business {BusinessId} assigned.",
                                accountId, existingAssignment.Id);
                        }

                        return existingAssignment;
                    }
                }
            }

            var businessesWithUserReviews = await businessReviewRepository
                .GetBaseQueryable()
                .Where(r => r.ReviewerId == accountId &&
                            (r.Status == BusinessReviewStatus.Pending ||
                             r.Status == BusinessReviewStatus.Accepted))
                .Select(r => r.BusinessId)
                .ToListAsync(cancellationToken);

            if (ApplicationEnvironment.Current().IsDevelopment)
            {
                logger.Debug("Found {Count} businesses with reviews from user {AccountId}",
                    businessesWithUserReviews.Count, accountId);
            }

            var skippedBusinesses = await GetSkippedBusinessesForUserAsync(accountId, cancellationToken);

            const int pageSize = 25;

            var baseQuery = businessRepository.GetBaseQueryable()
                .Include(b => b.OwnerAccount)
                .Where(b => b.OwnerAccount != null)
                .Where(b => b.OwnerAccount != null && b.OwnerAccount.Id != accountId)
                .Where(b => b.IsVerified)
                .Where(b => b.OwnerAccount != null && b.OwnerAccount.PointBalance >
                    PointConstants.MinimumOwnerPointBalanceForBusinessVisibility)
                .Where(b => !businessesWithUserReviews.Contains(b.Id))
                .Where(b => !skippedBusinesses.Contains(b.Id))
                .OrderBy(b => b.Priority)
                .ThenBy(b => b.Id);

            if (!await baseQuery.AnyAsync(cancellationToken))
            {
                logger.Information("No businesses available matching criteria for user {AccountId}", accountId);
                return null;
            }

            for (var attempt = 0; attempt < maxAttempts; attempt++)
            {
                var businesses = await baseQuery
                    .Skip(attempt * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

                if (businesses.Count == 0)
                {
                    logger.Information(
                        "No more businesses available after checking {Attempts} batches for user {AccountId}",
                        attempt, accountId);
                    return null;
                }

                foreach (var business in businesses)
                {
                    var hasReview = await businessReviewRepository
                        .GetBaseQueryable()
                        .AnyAsync(r => r.ReviewerId == accountId &&
                                       r.BusinessId == business.Id &&
                                       (r.Status == BusinessReviewStatus.Pending ||
                                        r.Status == BusinessReviewStatus.Accepted),
                            cancellationToken);

                    if (hasReview)
                    {
                        if (ApplicationEnvironment.Current().IsDevelopment)
                        {
                            logger.Debug("Skipping business {BusinessId} as user {AccountId} has already reviewed it",
                                business.Id, accountId);
                        }

                        continue;
                    }

                    if (business.OwnerAccount != null && business.OwnerAccount.Id == accountId)
                    {
                        if (ApplicationEnvironment.Current().IsDevelopment)
                        {
                            logger.Debug("Skipping business {BusinessId} as it's owned by user {AccountId}",
                                business.Id, accountId);
                        }

                        continue;
                    }

                    if (business.OwnerAccount is
                        { PointBalance: < PointConstants.MinimumOwnerPointBalanceForBusinessVisibility })
                    {
                        if (ApplicationEnvironment.Current().IsDevelopment)
                        {
                            logger.Debug(
                                "Skipping business {BusinessId} as owner has not enough points to be visible in the queue",
                                business.Id);
                        }

                        continue;
                    }

                    var assignSuccess = await AssignBusinessToUserAsync(business.Id, accountId);

                    if (!assignSuccess)
                        continue;

                    return business;
                }

                if (ApplicationEnvironment.Current().IsDevelopment)
                {
                    logger.Debug("All businesses in batch {Attempt} were locked, trying next batch", attempt);
                }
            }

            logger.Information("Could not find available business after {MaxAttempts} batches for user {AccountId}",
                maxAttempts, accountId);
            return null;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error finding available business for user {AccountId}", accountId);
            throw;
        }
    }

    public async Task<bool> SkipBusinessAssignmentAsync(long accountId,
        CancellationToken cancellationToken = default)
    {
        var currentlyAssignedBusiness = await GetUserAssignedBusinessAsync(accountId, cancellationToken);

        if (currentlyAssignedBusiness is null)
        {
            logger.Information("No business assigned to user {AccountId}", accountId);
            return false;
        }

        var lockAcquired = await distributedLockProvider.IsLockAcquiredAsync(
            DistributedLockCategory.BusinessQueue,
            currentlyAssignedBusiness.Id.ToString()
        );

        if (!lockAcquired)
        {
            logger.Information("Business {BusinessId} is not locked, skipping assignment for user {AccountId}",
                currentlyAssignedBusiness.Id, accountId);
            return false;
        }

        var skippedBusinesses = await GetSkippedBusinessesForUserAsync(accountId, cancellationToken);

        if (!skippedBusinesses.Contains(currentlyAssignedBusiness.Id))
        {
            skippedBusinesses.Add(currentlyAssignedBusiness.Id);
        }

        await redisCacheProvider.SetAsync(
            CacheEntryCategory.SkippedQueuedBusinessByUser,
            accountId.ToString(),
            skippedBusinesses,
            TimeSpan.FromMinutes(BusinessQueueConstants.SkippedBusinessCacheTimeoutInMinutes)
        );

        await distributedLockProvider.ForceReleaseLockAsync(
            DistributedLockCategory.BusinessQueue,
            currentlyAssignedBusiness.Id.ToString()
        );

        await redisCacheProvider.RemoveAsync(
            CacheEntryCategory.QueuedBusinessForUser,
            accountId.ToString()
        );

        logger.Information("Business {BusinessId} skipped for user {AccountId}",
            currentlyAssignedBusiness.Id, accountId);
        return true;
    }

    public async Task<bool> IsBusinessInUseAsync(long businessId, CancellationToken cancellationToken = default)
    {
        return await distributedLockProvider.IsLockAcquiredAsync(
            DistributedLockCategory.BusinessQueue,
            businessId.ToString()
        );
    }

    public async Task<BusinessEntity?> GetUserAssignedBusinessAsync(long accountId,
        CancellationToken cancellationToken = default)
    {
        var businessId = await redisCacheProvider.GetAsync<long?>(
            CacheEntryCategory.QueuedBusinessForUser,
            accountId.ToString()
        );

        if (businessId is null)
        {
            logger.Information("No business assigned to user {AccountId}", accountId);
            return null;
        }

        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var businessRepository = unitOfWork.GetRepository<BusinessEntity>();

        var business = await businessRepository.GetBaseQueryable()
            .Include(b => b.OwnerAccount)
            .FirstOrDefaultAsync(b => b.Id == businessId, cancellationToken);

        if (business is null)
        {
            logger.Information("Business {BusinessId} not found for user {AccountId}", businessId, accountId);
            return null;
        }

        return business;
    }

    public async Task<bool> IsBusinessAssignedToUserAsync(long businessId, long accountId,
        CancellationToken cancellationToken = default)
    {
        var assignedBusinessId = await redisCacheProvider.GetAsync<long?>(
            CacheEntryCategory.QueuedBusinessForUser,
            accountId.ToString()
        );

        if (assignedBusinessId is null)
        {
            logger.Information("No business assigned to user {AccountId}", accountId);
            return false;
        }

        if (assignedBusinessId != businessId)
        {
            logger.Information("Business {BusinessId} is not assigned to user {AccountId}", businessId, accountId);
            return false;
        }

        return true;
    }

    public async Task<TimeSpan?> GetAssignedBusinessLockTtlByUserAsync(long accountId,
        CancellationToken cancellationToken = default)
    {
        var cachedBusinessId = await redisCacheProvider.GetAsync<long?>(
            CacheEntryCategory.QueuedBusinessForUser,
            accountId.ToString()
        );

        if (long.TryParse(cachedBusinessId.ToString(), out var businessId) == false)
        {
            logger.Information("No business assigned to user {AccountId}", accountId);
            return null;
        }

        if (cachedBusinessId is null)
        {
            logger.Information("No business assigned to user {AccountId}", accountId);
            return null;
        }

        var ttl = await distributedLockProvider.GetLockTtlAsync(
            DistributedLockCategory.BusinessQueue,
            businessId.ToString()
        );

        return ttl;
    }

    public async Task<bool> AssignBusinessToUserAsync(
        long businessId,
        long accountId)
    {
        var lockAcquired = await distributedLockProvider.TryAcquirePersistentLockAsync(
            DistributedLockCategory.BusinessQueue,
            businessId.ToString(),
            TimeSpan.FromMinutes(BusinessQueueConstants.BusinessLockTimeoutInMinutes)
        );

        if (!lockAcquired)
        {
            logger.Information("Business {BusinessId} is already assigned to user {AccountId}", businessId, accountId);
            return false;
        }

        await redisCacheProvider.SetAsync(
            CacheEntryCategory.QueuedBusinessForUser,
            accountId.ToString(),
            businessId,
            TimeSpan.FromMinutes(BusinessQueueConstants.BusinessLockTimeoutInMinutes)
        );

        logger.Information("Assigned business {BusinessId} to user {AccountId}", businessId, accountId);

        return true;
    }

    public async Task<bool> UnassignBusinessFromUserAsync(
        long businessId,
        long accountId)
    {
        var lockAcquired = await distributedLockProvider.IsLockAcquiredAsync(
            DistributedLockCategory.BusinessQueue,
            businessId.ToString()
        );

        if (!lockAcquired)
        {
            logger.Information("Business {BusinessId} is already unassigned from user {AccountId}", businessId,
                accountId);
            return false;
        }

        await distributedLockProvider.ForceReleaseLockAsync(
            DistributedLockCategory.BusinessQueue,
            businessId.ToString()
        );

        await redisCacheProvider.RemoveAsync(
            CacheEntryCategory.QueuedBusinessForUser,
            accountId.ToString()
        );

        logger.Information("Unassigned business {BusinessId} from user {AccountId}", businessId, accountId);

        return true;
    }

    private async Task<List<long>> GetSkippedBusinessesForUserAsync(long accountId,
        CancellationToken cancellationToken = default)
    {
        var skippedBusinesses = await redisCacheProvider.GetAsync<List<long>>(
            CacheEntryCategory.SkippedQueuedBusinessByUser,
            accountId.ToString()
        );

        return skippedBusinesses ?? [];
    }
}