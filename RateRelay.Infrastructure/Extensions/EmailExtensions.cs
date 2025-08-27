using Microsoft.Extensions.DependencyInjection;
using RateRelay.Infrastructure.Interfaces;
using RateRelay.Infrastructure.Services;

namespace RateRelay.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddEmailServices(this IServiceCollection services)
    {
        services.AddSingleton<IEmailTemplateService, FluidEmailTemplateService>();
        services.AddSingleton<IEmailModelBuilderService, EmailModelBuilderService>();
        services.AddScoped<IEmailService, EmailService>();
    }
}