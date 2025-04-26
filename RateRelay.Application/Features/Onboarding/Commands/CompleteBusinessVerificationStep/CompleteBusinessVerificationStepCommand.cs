using MediatR;
using RateRelay.Application.DTOs.Onboarding.Commands.CompleteBusinessVerificationStep;

namespace RateRelay.Application.Features.Onboarding.Commands.CompleteBusinessVerificationStep;

public class CompleteBusinessVerificationStepCommand : IRequest<CompleteBusinessVerificationStepOutputDto>
{
    public required string PlaceId { get; set; }
}