using AutoMapper;

namespace RateRelay.Application.Mapping;

public class AccountProfile : Profile
{
    public AccountProfile()
    {
        CreateMap<Domain.Entities.AccountEntity, DTOs.Account.Queries.AccountQueryOutputDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src => src.Permissions))
            .ForMember(dest => dest.PointBalance, opt => opt.MapFrom(src => src.PointBalance));
    }
}