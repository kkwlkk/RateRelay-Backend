using AutoMapper;
using RateRelay.Application.DTOs.User.Account.Queries;

namespace RateRelay.Application.Mapping;

public class AccountProfile : Profile
{
    public AccountProfile()
    {
        CreateMap<Domain.Entities.AccountEntity, AccountQueryOutputDto>()
            .ForMember(dest => dest.Role, opt => opt.Ignore());
    }
}