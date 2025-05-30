using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RateRelay.Domain.Common;
using RateRelay.Domain.Enums;
using RateRelay.Domain.Interfaces;
using RateRelay.Infrastructure.Services;

namespace RateRelay.API.Attributes.Auth;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireOnboardingStepAttribute : TypeFilterAttribute
{
    public RequireOnboardingStepAttribute(AccountOnboardingStep requiredStep = AccountOnboardingStep.Completed)
        : base(typeof(OnboardingStepFilter))
    {
        Arguments = [requiredStep];
    }
}

public class OnboardingStepFilter(
    AccountOnboardingStep requiredStep,
    IOnboardingService onboardingService,
    CurrentUserContext currentUserContext)
    : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!currentUserContext.IsAuthenticated)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // For onboarding endpoints themselves, we need to check if the step is accessible
        var isOnboardingController = context.Controller.GetType().Name.Contains("Onboarding");

        if (isOnboardingController)
        {
            var isStepAccessible = await onboardingService.IsStepAccessibleAsync(
                currentUserContext.AccountId,
                requiredStep);

            if (!isStepAccessible)
            {
                var currentStep = await onboardingService.GetCurrentStepAsync(currentUserContext.AccountId);

                var response = ApiResponse<object>.Create(
                    false,
                    errorMessage:
                    $"You are currently at onboarding step {currentStep} and cannot access step {requiredStep}.",
                    errorCode: "ONBOARDING_STEP_INACCESSIBLE",
                    statusCode: 403);

                context.Result = new ObjectResult(response)
                {
                    StatusCode = 403
                };
                return;
            }
        }

        else
        {
            var currentStep = await onboardingService.GetCurrentStepAsync(currentUserContext.AccountId);

            if (currentStep < requiredStep)
            {
                var response = ApiResponse<object>.Create(
                    false,
                    errorMessage:
                    $"You must complete onboarding step {requiredStep} before accessing this resource.",
                    errorCode: "ONBOARDING_INCOMPLETE",
                    statusCode: 403);

                context.Result = new ObjectResult(response)
                {
                    StatusCode = 403
                };
                return;
            }
        }

        await next();
    }
}