using AutoMapper;
using RateRelay.Application.DTOs.Accounts.Commands;
using RateRelay.Application.DTOs.Queries.Accounts;
using RateRelay.Domain.Entities;

namespace RateRelay.Application.Mapping;

public class AccountProfile : Profile
{
    public AccountProfile()
    {
        CreateMap<AccountEntity, AccountsQueryResponseDto>()
            .ForMember(dest => dest.AccountId, opt => opt.MapFrom(src => src.Id));
        CreateMap<AccountEntity, CreateAccountCommandResponseDto>()
            .ForMember(dest => dest.AccountId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
            .ForMember(dest => dest.DateCreatedUtc, opt => opt.MapFrom(src => src.DateCreatedUtc));
    }
}