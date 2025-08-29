using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RateRelay.Infrastructure.Configuration;
using RateRelay.Infrastructure.Interfaces;
using RateRelay.Infrastructure.Services;
using RateRelay.Infrastructure.Services.Email;
using Serilog;

namespace RateRelay.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddEmailServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IEmailTemplateService, FluidEmailTemplateService>();
        services.AddSingleton<IEmailModelBuilderService, EmailModelBuilderService>();
        
        var emailOptions = configuration.GetSection(EmailOptions.SectionName).Get<EmailOptions>();
        if (emailOptions is null || !emailOptions.Enabled)
        {
            Log.Warning("EmailService is disabled in configuration. Using FakeEmailService.");
            services.AddSingleton<IEmailService, FakeEmailService>();
        }
        else
        {
            services.AddTransient<IEmailService, EmailService>();
        }
    }
}