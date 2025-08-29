using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RateRelay.Infrastructure.Interfaces;
using RateRelay.Domain.Common;
using RateRelay.Domain.Enums;
using RateRelay.Infrastructure.Services;

namespace RateRelay.API.Filters;

public class MaintenanceModeFilter(
    IMaintenanceModeService maintenanceModeService,
    CurrentUserContext currentUserContext) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var isInMaintenance = await maintenanceModeService.IsInMaintenanceModeAsync();
        var userHasBypassAccess = currentUserContext.IsAuthenticated && currentUserContext.HasPermission(Permission.BypassMaintenanceMode);

        if (!isInMaintenance || userHasBypassAccess)
        {
            await next();
            return;
        }
        
        var response = ApiResponse<object>.Create(
            success: false,
            errorMessage: "The service is currently under maintenance",
            errorCode: "MAINTENANCE_MODE",
            statusCode: 503
        );
        
        context.Result = new ObjectResult(response)
        {
            StatusCode = 503
        };
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class DisableDuringMaintenanceAttribute : Attribute, IFilterFactory
{
    public bool IsReusable => false;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        return serviceProvider.GetRequiredService<MaintenanceModeFilter>();
    }
}