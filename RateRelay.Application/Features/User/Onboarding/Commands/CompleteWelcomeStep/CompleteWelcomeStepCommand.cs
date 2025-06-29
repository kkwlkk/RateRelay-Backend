using MediatR;
using RateRelay.Application.DTOs.Onboarding.Commands.CompleteWelcomeStep;

namespace RateRelay.Application.Features.Onboarding.Commands.CompleteWelcomeStep;

public class CompleteWelcomeStepCommand : IRequest<CompleteWelcomeStepOutputDto>
{
    public bool AcceptedTerms { get; set; }
}