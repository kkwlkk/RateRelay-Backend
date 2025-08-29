using MediatR;
using RateRelay.Application.DTOs.Shared;

namespace RateRelay.Application.Features.Shared.Maintenance.Queries.GetMaintenanceMode;

public class GetMaintenanceModeQuery : IRequest<GetMaintenanceModeOutputDto>;