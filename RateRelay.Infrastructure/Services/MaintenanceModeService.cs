using Microsoft.EntityFrameworkCore;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Interfaces.DataAccess;
using RateRelay.Infrastructure.Interfaces;

namespace RateRelay.Infrastructure.Services;

public class MaintenanceModeService(IUnitOfWorkFactory unitOfWorkFactory) : IMaintenanceModeService
{
    public async Task<bool> IsInMaintenanceModeAsync(CancellationToken cancellationToken = default)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var maintenanceModeRepository = unitOfWork.GetRepository<MaintenanceModeEntity>();
        
        var maintenanceMode = await maintenanceModeRepository.GetBaseQueryable()
            .FirstOrDefaultAsync(m => m.IsActive, cancellationToken);
        
        return maintenanceMode is not null;
    }

    public async Task<MaintenanceModeEntity?> GetCurrentMaintenanceAsync(CancellationToken cancellationToken = default)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var maintenanceModeRepository = unitOfWork.GetRepository<MaintenanceModeEntity>();
        
        var maintenance = await maintenanceModeRepository.GetBaseQueryable()
            .FirstOrDefaultAsync(m => m.IsActive, cancellationToken);

        return maintenance;
    }
}