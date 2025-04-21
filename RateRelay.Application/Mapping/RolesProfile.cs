using AutoMapper;

namespace RateRelay.Application.Mapping;

public class RolesProfile : Profile
{
    public RolesProfile()
    {
        CreateMap<Domain.Entities.RoleEntity, DTOs.RoleEntityOutputDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src => src.Permissions));
    }
}