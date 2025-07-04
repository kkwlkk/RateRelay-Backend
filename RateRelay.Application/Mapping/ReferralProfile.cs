using AutoMapper;
using RateRelay.Application.DTOs.User.Referral;
using RateRelay.Application.DTOs.User.Referral.Commands;
using RateRelay.Application.DTOs.User.Referral.Queries;
using RateRelay.Application.Features.User.Referral.Commands.LinkReferral;
using RateRelay.Domain.Common.DTOs;
using RateRelay.Domain.Entities;

namespace RateRelay.Application.Mapping;

public class ReferralProfile : Profile
{
    public ReferralProfile()
    {
        CreateMap<LinkReferralInputDto, LinkReferralCommand>();

        CreateMap<ReferralStatsDto, ReferralStatsOutputDto>();

        CreateMap<ReferralProgressSummary, ReferralProgressDto>();

        CreateMap<ReferralGoalEntity, ReferralGoalOutputDto>()
            .ForMember(dest => dest.GoalType, opt => opt.MapFrom(src => src.GoalType.ToString()));
    }
}