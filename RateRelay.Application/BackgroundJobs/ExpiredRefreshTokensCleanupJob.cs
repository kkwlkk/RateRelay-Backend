using Microsoft.EntityFrameworkCore;
using RateRelay.Application.BackgroundJobs.Common;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Interfaces.DataAccess;
using RateRelay.Infrastructure.Hangfire;
using Serilog;

namespace RateRelay.Application.BackgroundJobs;

[HangfireRecurringJob(nameof(ExpiredRefreshTokensCleanupJob), "*/30 * * * *")]
public class ExpiredRefreshTokensCleanupJob(IUnitOfWorkFactory unitOfWorkFactory) : BaseHangfireJob
{
    public override async Task ExecuteAsync()
    {
        try
        {
            await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
            var refreshTokensRepository = unitOfWork.GetRepository<RefreshTokenEntity>();

            var cutoffDate = DateTime.UtcNow.AddDays(-30);
            var tokensToRemove = await refreshTokensRepository.GetBaseQueryable(true)
                .Where(token =>
                    token.DateDeletedUtc != null ||
                    (token.DateDeletedUtc == null && token.DateCreatedUtc < cutoffDate))
                .ToListAsync();

            if (tokensToRemove.Count == 0)
            {
                Logger.Information("No expired or used refresh tokens found for cleanup");
                return;
            }

            foreach (var token in tokensToRemove)
            {
                Logger.Information("Removing refresh token {TokenId} created on {DateCreatedUtc}",
                    token.Id, token.DateCreatedUtc);
                refreshTokensRepository.HardRemove(token);
            }

            await unitOfWork.SaveChangesAsync(true);

            Logger.Information("Removed {TokenCount} refresh tokens (expired or used) during cleanup",
                tokensToRemove.Count);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "An error occurred while cleaning up expired refresh tokens");
            throw;
        }
    }
}