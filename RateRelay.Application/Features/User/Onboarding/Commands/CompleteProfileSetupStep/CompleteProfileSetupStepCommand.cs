using MediatR;
using RateRelay.Application.DTOs.Onboarding.Commands.CompleteProfileSetup;

namespace RateRelay.Application.Features.User.Onboarding.Commands.CompleteProfileSetupStep;

public class CompleteProfileSetupStepCommand : IRequest<CompleteProfileSetupOutputDto>
{
    public string DisplayName { get; set; }
}