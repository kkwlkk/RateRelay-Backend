using MediatR;
using RateRelay.Application.DTOs.Onboarding;
using RateRelay.Application.DTOs.Onboarding.Queries.GetOnboardingStatus;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Enums;
using RateRelay.Domain.Interfaces.DataAccess;
using RateRelay.Infrastructure.Services;

namespace RateRelay.Application.Features.Onboarding.Queries.GetOnboardingStatus;

public class GetOnboardingStatusQueryHandler(
    CurrentUserContext currentUserContext,
    IUnitOfWorkFactory unitOfWorkFactory
) : IRequestHandler<GetOnboardingStatusQuery, GetOnboardingStatusOutputDto>
{
    public async Task<GetOnboardingStatusOutputDto> Handle(GetOnboardingStatusQuery request, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var accountRepository = unitOfWork.GetRepository<AccountEntity>();
        
        var account = await accountRepository.GetByIdAsync(currentUserContext.AccountId, cancellationToken);
        if (account == null)
        {
            throw new KeyNotFoundException($"Account not found.");
        }

        var result = new GetOnboardingStatusOutputDto
        {
            CurrentStep = account.OnboardingStep,
            CurrentStepName = GetStepName(account.OnboardingStep),
            LastUpdated = account.OnboardingLastUpdatedUtc,
            IsCompleted = account.HasCompletedOnboarding,
            CompletedSteps = GetCompletedSteps(account.OnboardingStep),
            RemainingSteps = GetRemainingSteps(account.OnboardingStep)
        };

        return result;
    }

    private static string GetStepName(AccountOnboardingStep step)
    {
        return step switch
        {
            AccountOnboardingStep.Welcome => "Welcome",
            AccountOnboardingStep.ProfileSetup => "Profile Setup",
            AccountOnboardingStep.BusinessVerification => "Business Verification",
            AccountOnboardingStep.Completed => "Completed",
            _ => "Unknown"
        };
    }

    private static List<string> GetCompletedSteps(AccountOnboardingStep currentStep)
    {
        var completedSteps = new List<string>();

        var currentStepValue = (int)currentStep;
        
        foreach (AccountOnboardingStep step in Enum.GetValues(typeof(AccountOnboardingStep)))
        {
            switch (step)
            {
                case AccountOnboardingStep.Completed when currentStep != AccountOnboardingStep.Completed:
                    continue;
            }

            if ((int)step < currentStepValue || step == currentStep)
            {
                completedSteps.Add(GetStepName(step));
            }
        }
        
        return completedSteps;
    }
    
    private static List<string> GetRemainingSteps(AccountOnboardingStep currentStep)
    {
        var remainingSteps = new List<string>();
        
        // Convert enum to int to easily compare
        var currentStepValue = (int)currentStep;
        
        foreach (AccountOnboardingStep step in Enum.GetValues(typeof(AccountOnboardingStep)))
        {
            if (step == AccountOnboardingStep.Welcome)
                continue;
                
            if ((int)step > currentStepValue)
            {
                remainingSteps.Add(GetStepName(step));
            }
        }
        
        return remainingSteps;
    }
}