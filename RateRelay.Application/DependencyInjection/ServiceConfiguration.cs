using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RateRelay.Application.MediatR;
using RateRelay.Infrastructure.Extensions;
using RateRelay.Infrastructure.Logging;

namespace RateRelay.Application.DependencyInjection;

public static class ServiceConfiguration
{
    public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddLogging(configuration);
        services.AddRateRelayDatabase(configuration);
        services.AddRateRelayAuth(configuration);
        services.AddMediatR();
        services.AddAutoMapperConfiguration();
    }
}