using MediatR;
using RateRelay.Application.DTOs.Onboarding.Commands.CompleteOnboardingStep;

namespace RateRelay.Application.Features.Onboarding.Commands.CompleteOnboardingStep;

public class CompleteOnboardingStepCommand : IRequest<CompleteOnboardingStepOutputDto>;