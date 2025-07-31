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
                var shouldKeepAssignment = await ValidateExistingAssignmentAsync(
                    existingAssignment, accountId, businessReviewRepository, cancellationToken);
                
                if (shouldKeepAssignment)
                {
                    return existingAssignment;
                }
            }

            var excludedBusinessIds = await GetExcludedBusinessIdsAsync(
                accountId, businessReviewRepository, cancellationToken);

            var skippedBusinesses = await GetSkippedBusinessesForUserAsync(accountId, cancellationToken);
            excludedBusinessIds.AddRange(skippedBusinesses);

            var baseQuery = BuildBaseBusinessQuery(businessRepository, accountId, excludedBusinessIds);

            if (!await baseQuery.AnyAsync(cancellationToken))
            {
                logger.Information("No businesses available matching criteria for user {AccountId}", accountId);
                return null;
            }

            return await FindAndAssignAvailableBusinessAsync(
                baseQuery, accountId, maxAttempts, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error finding available business for user {AccountId}", accountId);
            throw;
        }
    }

    private async Task<bool> ValidateExistingAssignmentAsync(
        BusinessEntity existingAssignment,
        long accountId,
        IRepository<BusinessReviewEntity> businessReviewRepository,
        CancellationToken cancellationToken)
    {
        if (existingAssignment.OwnerAccount?.Id == accountId)
        {
            if (ApplicationEnvironment.Current().IsDevelopment)
            {
                logger.Debug("User {AccountId} was assigned to their own business {BusinessId}. Finding a new business.",
                    accountId, existingAssignment.Id);
            }
            await UnassignBusinessFromUserAsync(existingAssignment.Id, accountId);
            return false;
        }

        var hasActiveReview = await businessReviewRepository
            .GetBaseQueryable()
            .AnyAsync(r => r.ReviewerId == accountId &&
                           r.BusinessId == existingAssignment.Id &&
                           (r.Status == BusinessReviewStatus.Pending ||
                            r.Status == BusinessReviewStatus.Accepted),
                cancellationToken);

        if (hasActiveReview)
        {
            logger.Information("User {AccountId} was assigned to business {BusinessId} they already reviewed. Finding a new business.",
                accountId, existingAssignment.Id);
            await UnassignBusinessFromUserAsync(existingAssignment.Id, accountId);
            return false;
        }

        if (ApplicationEnvironment.Current().IsDevelopment)
        {
            logger.Debug("User {AccountId} already has business {BusinessId} assigned.",
                accountId, existingAssignment.Id);
        }

        return true;
    }

    private async Task<List<long>> GetExcludedBusinessIdsAsync(
        long accountId,
        IRepository<BusinessReviewEntity> businessReviewRepository,
        CancellationToken cancellationToken)
    {
        var businessesWithActiveReviews = await businessReviewRepository
            .GetBaseQueryable()
            .Where(r => r.ReviewerId == accountId &&
                        (r.Status == BusinessReviewStatus.Pending ||
                         r.Status == BusinessReviewStatus.Accepted ||
                         r.Status == BusinessReviewStatus.UnderDispute))
            .Select(r => r.BusinessId)
            .ToListAsync(cancellationToken);

        var businessesWithThreeRejections = await businessReviewRepository
            .GetBaseQueryable()
            .Where(r => r.ReviewerId == accountId && r.Status == BusinessReviewStatus.Rejected)
            .GroupBy(r => r.BusinessId)
            .Where(g => g.Count() >= 3)
            .Select(g => g.Key)
            .ToListAsync(cancellationToken);

        var excludedBusinessIds = businessesWithActiveReviews.Union(businessesWithThreeRejections).ToList();

        if (ApplicationEnvironment.Current().IsDevelopment)
        {
            logger.Debug("Found {ActiveCount} businesses with active reviews and {RejectedCount} businesses with 3+ rejections for user {AccountId}",
                businessesWithActiveReviews.Count, businessesWithThreeRejections.Count, accountId);
        }

        return excludedBusinessIds;
    }

    private IQueryable<BusinessEntity> BuildBaseBusinessQuery(
        IRepository<BusinessEntity> businessRepository,
        long accountId,
        List<long> excludedBusinessIds)
    {
        return businessRepository.GetBaseQueryable()
            .Include(b => b.OwnerAccount)
            .Where(b => b.OwnerAccount != null && b.OwnerAccount.Id != accountId)
            .Where(b => b.IsVerified)
            .Where(b => b.OwnerAccount.PointBalance >= PointConstants.MinimumOwnerPointBalanceForBusinessVisibility)
            .Where(b => !excludedBusinessIds.Contains(b.Id))
            .OrderByDescending(b => b.Priority)
            .ThenBy(b => b.Id);
    }

    private async Task<BusinessEntity?> FindAndAssignAvailableBusinessAsync(
        IQueryable<BusinessEntity> baseQuery,
        long accountId,
        int maxAttempts,
        CancellationToken cancellationToken)
    {
        const int pageSize = 25;

        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            var businesses = await baseQuery
                .Skip(attempt * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            if (businesses.Count == 0)
            {
                logger.Information("No more businesses available after checking {Attempts} batches for user {AccountId}",
                    attempt, accountId);
                return null;
            }

            foreach (var business in businesses)
            {
                if (!IsBusinessEligibleForUser(business, accountId))
                    continue;

                var assignSuccess = await AssignBusinessToUserAsync(business.Id, accountId);
                if (assignSuccess)
                {
                    return business;
                }
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

    private bool IsBusinessEligibleForUser(BusinessEntity business, long accountId)
    {
        if (business.OwnerAccount?.Id == accountId)
        {
            if (ApplicationEnvironment.Current().IsDevelopment)
            {
                logger.Debug("Skipping business {BusinessId} as it's owned by user {AccountId}",
                    business.Id, accountId);
            }
            return false;
        }

        if (business.OwnerAccount?.PointBalance < PointConstants.MinimumOwnerPointBalanceForBusinessVisibility)
        {
            if (ApplicationEnvironment.Current().IsDevelopment)
            {
                logger.Debug("Skipping business {BusinessId} as owner has insufficient points for visibility",
                    business.Id);
            }
            return false;
        }

        return true;
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

        if (cachedBusinessId is null || string.IsNullOrEmpty(cachedBusinessId.ToString()))
        {
            logger.Information("No business assigned to user {AccountId}", accountId);
            return null;
        }

        var ttl = await distributedLockProvider.GetLockTtlAsync(
            DistributedLockCategory.BusinessQueue,
            cachedBusinessId.ToString()
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

    public async Task<bool> IsBusinessEligibleForQueueAsync(long businessId, CancellationToken cancellationToken = default)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var businessRepository = unitOfWork.GetRepository<BusinessEntity>();
        
        var business = await businessRepository.GetBaseQueryable()
            .Where(b => b.Id == businessId)
            .Include(b => b.OwnerAccount)
            .FirstOrDefaultAsync(cancellationToken);

        if (business is null)
        {
            logger.Information("Business {BusinessId} not found", businessId);
            return false;
        }
        
        if (!business.IsVerified) 
        {
            if (ApplicationEnvironment.Current().IsDevelopment)
            {
                logger.Debug("Business {BusinessId} is not verified", businessId);
            }

            return false;
        }

        if (business.OwnerAccount?.PointBalance < PointConstants.MinimumOwnerPointBalanceForBusinessVisibility)
        {
            if (ApplicationEnvironment.Current().IsDevelopment)
            {
                logger.Debug("Business {BusinessId} is not accessible due to insufficient owner points", businessId);
            }

            return false;
        }
        
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