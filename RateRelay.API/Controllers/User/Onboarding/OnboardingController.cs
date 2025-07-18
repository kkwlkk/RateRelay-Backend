using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RateRelay.API.Attributes.Auth;
using RateRelay.Application.DTOs.Onboarding.Commands.CompleteBusinessVerificationStep;
using RateRelay.Application.DTOs.Onboarding.Commands.CompleteOnboardingStep;
using RateRelay.Application.DTOs.Onboarding.Commands.CompleteProfileSetup;
using RateRelay.Application.DTOs.Onboarding.Commands.CompleteWelcomeStep;
using RateRelay.Application.DTOs.Onboarding.Queries.GetOnboardingStatus;
using RateRelay.Application.Features.Onboarding.Commands.CompleteBusinessVerificationStep;
using RateRelay.Application.Features.Onboarding.Commands.CompleteOnboardingStep;
using RateRelay.Application.Features.Onboarding.Commands.CompleteWelcomeStep;
using RateRelay.Application.Features.Onboarding.Queries.GetOnboardingStatus;
using RateRelay.Application.Features.User.Onboarding.Commands.CompleteProfileSetupStep;
using RateRelay.Domain.Enums;

namespace RateRelay.API.Controllers.User.Onboarding;

[ApiController]
[Area("Onboarding")]
[Authorize]
public class OnboardingController(IMediator mediator, IMapper mapper) : UserBaseController
{
    [HttpGet("status")]
    [ProducesResponseType(typeof(GetOnboardingStatusOutputDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOnboardingStatus(CancellationToken cancellationToken)
    {
        var query = new GetOnboardingStatusQuery();
        var onboardingStatus = await mediator.Send(query, cancellationToken);
        return Success(onboardingStatus);
    }

    [HttpPost("welcome")]
    [RequireOnboardingStep(AccountOnboardingStep.Welcome)]
    [ProducesResponseType(typeof(CompleteWelcomeStepOutputDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> CompleteWelcomeStep(
        [FromBody] CompleteWelcomeStepInputDto input,
        CancellationToken cancellationToken)
    {
        var command = mapper.Map<CompleteWelcomeStepCommand>(input);
        var result = await mediator.Send(command, cancellationToken);
        return Success(result);
    }

    [HttpPost("profile-setup")]
    [RequireOnboardingStep(AccountOnboardingStep.ProfileSetup)]
    [ProducesResponseType(typeof(CompleteProfileSetupOutputDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> CompleteProfileSetupStep(
        [FromBody] CompleteProfileSetupInputDto input,
        CancellationToken cancellationToken)
    {
        var command = mapper.Map<CompleteProfileSetupStepCommand>(input);
        var result = await mediator.Send(command, cancellationToken);
        return Success(result);
    }

    [HttpPost("business-verification")]
    [RequireOnboardingStep(AccountOnboardingStep.BusinessVerification)]
    public async Task<IActionResult> CompleteBusinessVerificationStep(
        [FromBody] CompleteBusinessVerificationStepInputDto input,
        CancellationToken cancellationToken)
    {
        var command = mapper.Map<CompleteBusinessVerificationStepCommand>(input);
        var result = await mediator.Send(command, cancellationToken);
        return Success(result);
    }

    [HttpPost("complete")]
    [RequireOnboardingStep(AccountOnboardingStep.Completed)]
    [ProducesResponseType(typeof(CompleteOnboardingStepOutputDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> CompleteOnboardingStep(CancellationToken cancellationToken)
    {
        var command = new CompleteOnboardingStepCommand();
        var result = await mediator.Send(command, cancellationToken);
        return Success(result);
    }
}