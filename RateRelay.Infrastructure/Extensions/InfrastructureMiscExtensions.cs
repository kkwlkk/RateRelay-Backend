using Microsoft.Extensions.DependencyInjection;
using RateRelay.Domain.Interfaces;
using RateRelay.Infrastructure.Services;

namespace RateRelay.Infrastructure.Extensions;

public static class InfrastructureMiscExtensions
{
    public static void AddInfrastructureMiscExtensions(this IServiceCollection services)
    {
        services.AddSingleton<IPointService, PointService>();
        services.AddScoped<IBusinessVerificationService, BusinessVerificationService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IBusinessQueueService, BusinessQueueService>();
        services.AddScoped<IGoogleMapsService, GoogleMapsService>();
        services.AddScoped<IOnboardingService, OnboardingService>();
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<IReferralService, ReferralService>();
    }
}