using Microsoft.EntityFrameworkCore;
using RateRelay.Application.BackgroundJobs.Common;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Interfaces.DataAccess;
using RateRelay.Infrastructure.Hangfire;

namespace RateRelay.Application.BackgroundJobs;

// [HangfireRecurringJob(nameof(TestHangfireJob), "*/1 * * * *")]
public class TestHangfireJob(IUnitOfWorkFactory unitOfWorkFactory) : BaseHangfireJob
{
    public override async Task ExecuteAsync()
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var userRepository = unitOfWork.GetRepository<AccountEntity>();

        var firstUser = await userRepository.GetBaseQueryable()
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync();

        if (firstUser is null)
        {
            Logger.Warning("No users found in the database.");
            return;
        }

        Logger.Information("First user: {@FirstUser}", firstUser);
    }
}