using Hangfire;
using Microsoft.Extensions.DependencyInjection;

namespace RateRelay.Infrastructure.Hangfire;

public class HangfireJobActivator(IServiceProvider serviceProvider) : JobActivator
{
    public override object ActivateJob(Type jobType)
    {
        return serviceProvider.GetRequiredService(jobType);
    }
}