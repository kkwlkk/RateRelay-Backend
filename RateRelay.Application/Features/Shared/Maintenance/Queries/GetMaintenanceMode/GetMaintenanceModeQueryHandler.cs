using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RateRelay.Application.DTOs.Shared;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Interfaces.DataAccess;
using RateRelay.Infrastructure.Interfaces;

namespace RateRelay.Application.Features.Shared.Maintenance.Queries.GetMaintenanceMode;

public class GetMaintenanceModeQueryHandler(IMaintenanceModeService maintenanceModeService, IMapper mapper)
    : IRequestHandler<GetMaintenanceModeQuery, GetMaintenanceModeOutputDto?>
{
    public async Task<GetMaintenanceModeOutputDto?> Handle(GetMaintenanceModeQuery request,
        CancellationToken cancellationToken)
    {
        var maintenanceMode = await maintenanceModeService.GetCurrentMaintenanceAsync(cancellationToken);

        if (maintenanceMode is null)
            return new GetMaintenanceModeOutputDto { IsActive = false };

        var output = mapper.Map<GetMaintenanceModeOutputDto>(maintenanceMode);

        return output;
    }
}