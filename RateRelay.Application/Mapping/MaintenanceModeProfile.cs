using AutoMapper;

namespace RateRelay.Application.Mapping;

public class MaintenanceModeProfile : Profile
{
    public MaintenanceModeProfile()
    {
        CreateMap<Domain.Entities.MaintenanceModeEntity, DTOs.Shared.GetMaintenanceModeOutputDto>();
    }
}