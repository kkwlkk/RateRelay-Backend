using Hangfire.Dashboard;
using Microsoft.Extensions.DependencyInjection;
using RateRelay.Domain.Enums;
using RateRelay.Infrastructure.Extensions;
using RateRelay.Infrastructure.Services;

namespace RateRelay.Infrastructure.Authorization;

public class HangfireAuthorizationFilter(IServiceProvider serviceProvider) : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        if (httpContext.Request.IsLocal())
            return true;

        using var scope = serviceProvider.CreateScope();
        var currentUserContext = scope.ServiceProvider.GetRequiredService<CurrentUserContext>();

        return currentUserContext.IsAuthenticated && currentUserContext.HasPermission(Permission.AccessHangfireDashboard);
    }
}