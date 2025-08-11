using Microsoft.EntityFrameworkCore;
using RateRelay.Application.BackgroundJobs.Common;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Interfaces.DataAccess;
using RateRelay.Infrastructure.Hangfire;

namespace RateRelay.Application.BackgroundJobs;

[HangfireRecurringJob(nameof(ExpiredBansCleanupJob), "*/5 * * * *")]
public class ExpiredBansCleanupJob(IUnitOfWorkFactory unitOfWorkFactory) : BaseHangfireJob
{
    public override async Task ExecuteAsync()
    {
        try
        {
            await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
            var userBansRepository = unitOfWork.GetRepository<AccountBanEntity>();

            var expiredBans = await userBansRepository.GetBaseQueryable()
                .Where(ban => ban.ExpiresAtUtc < DateTime.UtcNow)
                .ToListAsync();

            if (expiredBans.Count == 0)
            {
                Logger.Information("No expired bans found.");
                return;
            }

            foreach (var ban in expiredBans)
            {
                userBansRepository.Remove(ban);
            }

            await unitOfWork.SaveChangesAsync();
            Logger.Information($"Removed {expiredBans.Count} expired bans.");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "An error occurred while cleaning up expired bans.");
            throw;
        }
    }
}