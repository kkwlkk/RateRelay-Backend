using AutoMapper;
using RateRelay.Application.DTOs.Business.BusinessReviews.Commands;
using RateRelay.Application.DTOs.Business.BusinessReviews.Queries;
using RateRelay.Application.DTOs.Business.BusinessVerification.Commands;
using RateRelay.Application.DTOs.Business.BusinessVerification.Queries;
using RateRelay.Application.DTOs.User.Business.BusinessReviews.Commands;
using RateRelay.Application.DTOs.User.Business.UserBusiness.Queries;
using RateRelay.Application.Features.Business.Commands.AcceptPendingBusinessReview;
using RateRelay.Application.Features.Business.Commands.InitiateBusinessVerification;
using RateRelay.Application.Features.User.Business.Commands.AcceptPendingBusinessReview;
using RateRelay.Application.Features.User.Business.Commands.ReportBusinessReview;
using RateRelay.Application.Features.User.Business.Queries.GetAllUserBusinesses;
using RateRelay.Application.Features.User.Business.Queries.GetBusinessReviews;
using RateRelay.Domain.Entities;

namespace RateRelay.Application.Mapping;

public class BusinessProfile : Profile
{
    public BusinessProfile()
    {
        CreateMap<InitiateBusinessVerificationInputDto, InitiateBusinessVerificationCommand>()
            .ForMember(dest => dest.PlaceId, opt => opt.MapFrom(src => src.PlaceId));

        CreateMap<BusinessVerificationEntity, BusinessVerificationOutputDto>()
            // .ForMember(dest => dest.PlaceId,
            // opt => opt.MapFrom(src => src.Business != null ? src.Business.PlaceId : string.Empty))
            .ForMember(dest => dest.VerificationId, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => GetVerificationStatus(src)))
            .ForMember(dest => dest.VerificationDay, opt => opt.MapFrom(src => (byte)src.VerificationDay));

        CreateMap<BusinessVerificationEntity, BusinessVerificationStatusOutputDto>()
            .ForMember(dest => dest.VerificationId, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => GetVerificationStatus(src)));

        CreateMap<BusinessVerificationEntity, BusinessVerificationChallengeOutputDto>()
            .ForMember(dest => dest.VerificationDay, opt => opt.MapFrom(src => (byte)src.VerificationDay))
            .ForMember(dest => dest.VerificationOpeningTime, opt => opt.MapFrom(src => src.VerificationOpeningTime))
            .ForMember(dest => dest.VerificationClosingTime, opt => opt.MapFrom(src => src.VerificationClosingTime));

        CreateMap<BusinessReviewEntity, GetAwaitingBusinessReviewsOutputDto>()
            .ForMember(dest => dest.ReviewId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.SubmittedAt, opt => opt.MapFrom(src => src.DateCreatedUtc));

        CreateMap<GetAllUserBusinessesQueryInputDto, GetAllUserBusinessesQuery>();

        CreateMap<GetBusinessReviewsQueryInputDto, GetBusinessReviewsQuery>();

        CreateMap<BusinessEntity, GetBusinessQueryOutputDto>()
            .ForMember(dest => dest.AverageRating, opt => opt.Ignore())
            .ForMember(dest => dest.Reviews, opt => opt.Ignore());

        CreateMap<BusinessReviewEntity, GetBusinessReviewsQueryOutputDto>()
            .ForMember(dest => dest.Comment, opt => opt.MapFrom(src => src.Comment.Trim()))
            .ForMember(dest => dest.PostedGoogleMapsReview, opt => opt.MapFrom(src => src.PostedGoogleReview))
            .ForMember(dest => dest.GoogleMapsReviewUrl, opt => opt.Ignore());
        
        CreateMap<ReportBusinessReviewInputDto, ReportBusinessReviewCommand>()
            .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content.Trim()))
            .ForMember(dest => dest.Reason, opt => opt.MapFrom(src => src.Reason));
    }

    private string GetVerificationStatus(BusinessVerificationEntity entity)
    {
        if (entity.VerificationCompletedUtc.HasValue)
            return "Completed";
        if (entity.IsVerificationExpired)
            return "Expired";
        return "Pending";
    }
}