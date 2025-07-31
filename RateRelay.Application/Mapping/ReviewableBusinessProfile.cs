using AutoMapper;
using RateRelay.Application.DTOs.ReviewableBusiness.Queries;
using RateRelay.Application.Features.ReviewableBusiness.Commands.SubmitBusinessReview;
using RateRelay.Application.Features.User.ReviewableBusiness.Queries.GetNextBusinessForReview;

namespace RateRelay.Application.Mapping;

public class ReviewableBusinessProfile : Profile
{
    public ReviewableBusinessProfile()
    {
        CreateMap<Domain.Entities.BusinessEntity, GetNextBusinessForReviewOutputDto>()
            .ForMember(dest => dest.BusinessId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.PlaceId, opt => opt.MapFrom(src => src.PlaceId))
            .ForMember(dest => dest.Cid, opt => opt.MapFrom(src => src.Cid))
            .ForMember(dest => dest.BusinessName, opt => opt.MapFrom(src => src.BusinessName))
            .ForMember(dest => dest.MapUrl, opt => opt.Ignore());

        CreateMap<Domain.Entities.BusinessReviewEntity,
                DTOs.ReviewableBusiness.Commands.SubmitBusinessReviewOutputDto>()
            .ForMember(dest => dest.ReviewId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.BusinessId, opt => opt.MapFrom(src => src.BusinessId))
            .ForMember(dest => dest.SubmittedOn, opt => opt.MapFrom(src => src.DateCreatedUtc));

        CreateMap<GetNextBusinessForReviewInputDto, GetNextBusinessForReviewQuery>()
            .ForMember(dest => dest.SkipBusinessAssignment, opt => opt.MapFrom(src => src.SkipBusiness));
        
        CreateMap<DTOs.ReviewableBusiness.Commands.SubmitBusinessReviewInputDto, SubmitBusinessReviewCommand>();
    }
}