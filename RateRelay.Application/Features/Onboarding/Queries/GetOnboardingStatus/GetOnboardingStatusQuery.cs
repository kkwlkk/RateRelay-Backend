using MediatR;
using RateRelay.Application.DTOs.Onboarding.Queries.GetOnboardingStatus;

namespace RateRelay.Application.Features.Onboarding.Queries.GetOnboardingStatus;

public class GetOnboardingStatusQuery : IRequest<GetOnboardingStatusOutputDto>;