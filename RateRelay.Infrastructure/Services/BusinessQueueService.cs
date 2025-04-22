using Microsoft.EntityFrameworkCore;
using RateRelay.Domain.Constants;
using RateRelay.Domain.Entities;
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
    private const int BusinessLockTimeoutInMinutes = 10;

    public async Task<BusinessEntity?> GetNextAvailableBusinessForUserAsync(
        long accountId,
        int maxAttempts = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var existingAssignment = await GetUserAssignedBusinessAsync(accountId, cancellationToken);
            if (existingAssignment != null)
            {
                logger.Information("User {AccountId} already has business {BusinessId} assigned",
                    accountId, existingAssignment.Id);
                return existingAssignment;
            }

            await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
            var businessRepository = unitOfWork.GetRepository<BusinessEntity>();

            const int pageSize = 25;

            var baseQuery = businessRepository.GetBaseQueryable()
                .Include(b => b.OwnerAccount)
                .Where(b => b.OwnerAccount != null &&
                            b.OwnerAccount.Id != accountId &&
                            b.IsVerified &&
                            b.OwnerAccount.PointBalance > PointConstants.MinimumOwnerPointBalanceForBusinessVisibility)
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
            CacheEntryCategory.QueuedBusinessUserLink,
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
            CacheEntryCategory.QueuedBusinessUserLink,
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
            CacheEntryCategory.QueuedBusinessUserLink,
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

    private async Task<bool> AssignBusinessToUserAsync(
        long businessId,
        long accountId)
    {
        var lockAcquired = await distributedLockProvider.TryAcquirePersistentLockAsync(
            DistributedLockCategory.BusinessQueue,
            businessId.ToString(),
            TimeSpan.FromMinutes(BusinessLockTimeoutInMinutes)
        );

        if (!lockAcquired)
        {
            logger.Information("Business {BusinessId} is already assigned to user {AccountId}", businessId, accountId);
            return false;
        }

        await redisCacheProvider.SetAsync(
            CacheEntryCategory.QueuedBusinessUserLink,
            accountId.ToString(),
            businessId,
            TimeSpan.FromMinutes(BusinessLockTimeoutInMinutes)
        );

        logger.Information("Assigned business {BusinessId} to user {AccountId}", businessId, accountId);

        return true;
    }

    private async Task<bool> UnassignBusinessFromUserAsync(
        long businessId,
        long accountId)
    {
        var lockAcquired = await distributedLockProvider.TryAcquirePersistentLockAsync(
            DistributedLockCategory.BusinessQueue,
            businessId.ToString(),
            TimeSpan.FromMinutes(BusinessLockTimeoutInMinutes)
        );

        if (!lockAcquired)
        {
            logger.Information("Business {BusinessId} is already unassigned from user {AccountId}", businessId,
                accountId);
            return false;
        }

        await redisCacheProvider.RemoveAsync(
            CacheEntryCategory.QueuedBusinessUserLink,
            accountId.ToString()
        );

        logger.Information("Unassigned business {BusinessId} from user {AccountId}", businessId, accountId);

        return true;
    }
}