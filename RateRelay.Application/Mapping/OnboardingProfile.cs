using AutoMapper;
using RateRelay.Application.DTOs.Onboarding.Commands.CompleteBusinessVerificationStep;
using RateRelay.Application.DTOs.Onboarding.Commands.CompleteWelcomeStep;
using RateRelay.Application.DTOs.Onboarding.Queries.GetOnboardingStatus;
using RateRelay.Application.Features.Onboarding.Commands.CompleteBusinessVerificationStep;
using RateRelay.Application.Features.User.Onboarding.Commands.CompleteWelcomeStep;
using RateRelay.Domain.Entities;

namespace RateRelay.Application.Mapping;

public class OnboardingProfile : Profile
{
    public OnboardingProfile()
    {
        CreateMap<CompleteWelcomeStepInputDto, CompleteWelcomeStepCommand>();

        CreateMap<AccountEntity, GetOnboardingStatusOutputDto>()
            .ForMember(dest => dest.CurrentStep, opt => opt.MapFrom(src => src.OnboardingStep))
            .ForMember(dest => dest.LastUpdated, opt => opt.MapFrom(src => src.OnboardingLastUpdatedUtc))
            .ForMember(dest => dest.IsCompleted, opt => opt.MapFrom(src => src.HasCompletedOnboarding));
        
        CreateMap<CompleteWelcomeStepCommand, CompleteWelcomeStepOutputDto>();
        
        CreateMap<CompleteBusinessVerificationStepInputDto, CompleteBusinessVerificationStepCommand>();
    }
}