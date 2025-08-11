using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RateRelay.Application.BackgroundJobs.Common;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Enums;
using RateRelay.Domain.Interfaces;
using RateRelay.Domain.Interfaces.DataAccess;
using RateRelay.Infrastructure.Hangfire;

namespace RateRelay.Application.BackgroundJobs;

[HangfireRecurringJob(nameof(AutoAcceptOverduePendingBusinessReviewsJob), "5 0 * * *")]
public class AutoAcceptOverduePendingBusinessReviewsJob(
    IUnitOfWorkFactory unitOfWorkFactory,
    IServiceProvider serviceProvider
) : BaseHangfireJob
{
    public override async Task ExecuteAsync()
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var reviewService = scope.ServiceProvider.GetRequiredService<IReviewService>();

            await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
            var reviewRepository = unitOfWork.GetRepository<BusinessReviewEntity>();

            var overdueReviews = await reviewRepository.GetBaseQueryable()
                .Where(review => review.Status == BusinessReviewStatus.Pending &&
                                 review.DateCreatedUtc < DateTime.UtcNow.AddDays(-7))
                .ToListAsync();

            if (overdueReviews.Count == 0)
            {
                Logger.Information("No overdue pending business reviews found.");
                return;
            }

            foreach (var review in overdueReviews)
            {
                await reviewService.AcceptUserReviewAsync(review.Id);
            }

            Logger.Information("Accepted {ReviewCount} overdue pending business reviews.", overdueReviews.Count);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "An error occurred while auto-accepting overdue pending business reviews.");
            throw;
        }
    }
}