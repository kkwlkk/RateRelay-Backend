using AutoMapper;
using RateRelay.Application.DTOs.Business.BusinessVerification.Commands;
using RateRelay.Application.DTOs.Business.BusinessVerification.Queries;
using RateRelay.Application.Features.Business.Commands.InitiateBusinessVerification;
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