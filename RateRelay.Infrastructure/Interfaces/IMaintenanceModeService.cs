using RateRelay.Domain.Entities;

namespace RateRelay.Infrastructure.Interfaces;

public interface IMaintenanceModeService
{
    Task<bool> IsInMaintenanceModeAsync(CancellationToken cancellationToken = default);
    Task<MaintenanceModeEntity?> GetCurrentMaintenanceAsync(CancellationToken cancellationToken = default);
}