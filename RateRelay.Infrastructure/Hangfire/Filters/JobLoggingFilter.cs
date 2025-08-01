using System.Diagnostics;
using Hangfire.Common;
using Hangfire.Server;
using RateRelay.Domain.Enums;
using RateRelay.Infrastructure.Extensions;
using Serilog;

namespace RateRelay.Infrastructure.Hangfire.Filters;

public class JobLoggingFilter : IJobFilter, IServerFilter
{
    private readonly Stopwatch _stopwatch = new();

    public void OnPerforming(PerformingContext context)
    {
        _stopwatch.Restart();
    }

    public void OnPerformed(PerformedContext context)
    {
        _stopwatch.Stop();

        var elapsedTime = _stopwatch.Elapsed.TotalMilliseconds;
        var jobType = context.BackgroundJob.Job.Type?.Name ?? "UnknownJob";

        if (context.Exception is null)
        {
            Log.Logger.Information(LogPrefix.HANGFIRE, $"Job {jobType} succeeded in {elapsedTime:F2} ms.");
        }
        else
        {
            Log.Logger.Error(LogPrefix.HANGFIRE,
                $"Job {jobType} failed after {elapsedTime:F2} ms. Error: {context.Exception.Message}");
        }
    }

    public bool AllowMultiple { get; }
    public int Order { get; }
}