using RateRelay.Domain.Interfaces;
using RateRelay.Infrastructure.Logging;
using Serilog;

namespace RateRelay.Application.BackgroundJobs.Common;

public abstract class BaseHangfireJob : IHangfireJob
{
    protected IScopedLogger Logger { get; } = LoggerFactory.CreateHangfireLogger(Log.Logger);

    public abstract Task ExecuteAsync();
}