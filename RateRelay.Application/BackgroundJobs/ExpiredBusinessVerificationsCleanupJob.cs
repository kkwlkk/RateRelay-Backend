using Microsoft.EntityFrameworkCore;
using RateRelay.Application.BackgroundJobs.Common;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Interfaces.DataAccess;
using RateRelay.Infrastructure.Hangfire;

namespace RateRelay.Application.BackgroundJobs;

[HangfireRecurringJob(nameof(ExpiredBusinessVerificationsCleanupJob), "5 */6 * * *")]
public class ExpiredBusinessVerificationsCleanupJob(
    IUnitOfWorkFactory unitOfWorkFactory
) : BaseHangfireJob
{
    public override async Task ExecuteAsync()
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var businessVerificationsRepository = unitOfWork.GetRepository<BusinessVerificationEntity>();
        var businessRepository = unitOfWork.GetRepository<BusinessEntity>();

        const int windowMinutes = Domain.Constants.BusinessVerificationConstants.BusinessVerificationWindowInMinutes;
        var verificationCutoffTime = DateTime.UtcNow.AddMinutes(-windowMinutes);

        var expiredVerifications = await businessVerificationsRepository.GetBaseQueryable()
            .Where(bv => bv.VerificationCompletedUtc == null && bv.VerificationStartedUtc < verificationCutoffTime)
            .ToListAsync();

        if (expiredVerifications.Count == 0)
        {
            Logger.Information("No expired business verifications found.");
            return;
        }

        var businessIds = expiredVerifications.Select(v => v.BusinessId).Distinct().ToList();
        var businesses = await businessRepository.GetBaseQueryable()
            .Where(b => businessIds.Contains(b.Id))
            .ToListAsync();

        var businessLookup = businesses.ToDictionary(b => b.Id);
        var removedVerificationsCount = 0;
        var removedBusinessesCount = 0;

        foreach (var businessId in businessIds)
        {
            if (!businessLookup.TryGetValue(businessId, out var business))
            {
                Logger.Warning("Business {BusinessId} not found. Removing orphaned verifications.", businessId);
                var orphanedVerifications = expiredVerifications.Where(v => v.BusinessId == businessId);
                foreach (var verification in orphanedVerifications)
                {
                    businessVerificationsRepository.Remove(verification);
                    removedVerificationsCount++;
                }

                continue;
            }

            if (business.IsVerified)
            {
                Logger.Information("Business {BusinessId} is verified. Removing only expired verifications.",
                    businessId);
                var expiredForBusiness = expiredVerifications.Where(v => v.BusinessId == businessId);
                foreach (var verification in expiredForBusiness)
                {
                    businessVerificationsRepository.Remove(verification);
                    removedVerificationsCount++;
                }

                continue;
            }

            Logger.Information("Business {BusinessId} is not verified. Removing business and all verifications.",
                businessId);

            var allVerificationsForBusiness = await businessVerificationsRepository.GetBaseQueryable()
                .Where(bv => bv.BusinessId == businessId)
                .ToListAsync();

            foreach (var verification in allVerificationsForBusiness)
            {
                businessVerificationsRepository.Remove(verification);
                removedVerificationsCount++;
            }

            businessRepository.Remove(business);
            removedBusinessesCount++;
        }

        await unitOfWork.SaveChangesAsync();
        Logger.Information(
            "Cleanup completed. Removed {VerificationCount} verifications and {BusinessCount} businesses.",
            removedVerificationsCount, removedBusinessesCount);
    }
}