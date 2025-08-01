using Hangfire;
using Hangfire.Storage;
using Microsoft.Extensions.DependencyInjection;
using RateRelay.Domain.Enums;
using RateRelay.Infrastructure.Extensions;
using Serilog;

namespace RateRelay.Infrastructure.Hangfire;

public static class HangfireCleanupExtensions
{
    public static void CleanupOrphanedJobs(IServiceProvider serviceProvider)
    {
        var recurringJobs = JobStorage.Current.GetConnection().GetRecurringJobs();
        var orphanedJobIds = new List<string>();

        using var scope = serviceProvider.CreateScope();

        foreach (var job in recurringJobs)
        {
            var jobType = Type.GetType(job.Job.Type.AssemblyQualifiedName ?? "");
            if (jobType is null || scope.ServiceProvider.GetService(jobType) is null)
            {
                orphanedJobIds.Add(job.Id);
            }
        }

        foreach (var jobId in orphanedJobIds)
        {
            RecurringJob.RemoveIfExists(jobId);
            Log.Logger.Information(LogPrefix.HANGFIRE, $"Removed orphaned recurring job: {jobId}");
        }
    }
}