using AutoMapper;
using RateRelay.Application.Features.Account.Queries.ReviewHistory;

namespace RateRelay.Application.Mapping;

public class AccountBusinessReviewsProfile : Profile
{
    public AccountBusinessReviewsProfile()
    {
        CreateMap<DTOs.Account.ReviewHistory.Queries.AccountReviewHistoryQueryInputDto, GetAccountReviewHistoryQuery>();

        CreateMap<Domain.Entities.BusinessReviewEntity, DTOs.Account.ReviewHistory.Queries.AccountReviewHistoryQueryOutputDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.BusinessName, opt => opt.MapFrom(src => src.Business.BusinessName))
            .ForMember(dest => dest.Cid, opt => opt.MapFrom(src => src.Business.Cid))
            .ForMember(dest => dest.MapUrl, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.DateCreatedUtc, opt => opt.MapFrom(src => src.DateCreatedUtc));
    }
}