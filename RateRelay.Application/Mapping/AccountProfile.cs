using AutoMapper;
using RateRelay.Application.DTOs.Queries.Accounts;
using RateRelay.Application.Features.Queries.Accounts;
using RateRelay.Domain.Entities;

namespace RateRelay.Application.Mapping;

public class AccountProfile : Profile
{
    public AccountProfile()
    {
        CreateMap<AccountEntity, AccountsQueryResponseDto>()
            .ForMember(dest => dest.AccountId, opt => opt.MapFrom(src => src.Id));
    }
}