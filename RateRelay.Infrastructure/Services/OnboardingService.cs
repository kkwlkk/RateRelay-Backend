using Microsoft.EntityFrameworkCore;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Enums;
using RateRelay.Domain.Exceptions;
using RateRelay.Domain.Interfaces;
using RateRelay.Domain.Interfaces.DataAccess;
using Serilog;

namespace RateRelay.Infrastructure.Services;

public class OnboardingService(
    IUnitOfWorkFactory unitOfWorkFactory,
    IReferralService referralService,
    ILogger logger
) : IOnboardingService
{
    public async Task<AccountOnboardingStep> GetCurrentStepAsync(long accountId,
        CancellationToken cancellationToken = default)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var accountRepository = unitOfWork.GetRepository<AccountEntity>();

        var account = await accountRepository.GetByIdAsync(accountId, cancellationToken);
        if (account == null)
        {
            throw new NotFoundException($"Account with ID {accountId} not found.");
        }

        return account.OnboardingStep;
    }

    public async Task<bool> UpdateStepAsync(long accountId, AccountOnboardingStep step,
        CancellationToken cancellationToken = default)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var accountRepository = unitOfWork.GetRepository<AccountEntity>();

        var account = await accountRepository.GetByIdAsync(accountId, cancellationToken);
        if (account == null)
        {
            throw new NotFoundException($"Account with ID {accountId} not found.");
        }

        var isValid = await ValidateStepTransitionAsync(accountId, account.OnboardingStep, step, cancellationToken);
        if (!isValid)
        {
            logger.Warning(
                "Invalid onboarding step transition attempted. AccountId: {AccountId}, Current: {CurrentStep}, Requested: {RequestedStep}",
                accountId, account.OnboardingStep, step);
            throw new InvalidOperationException("Invalid onboarding step transition.");
        }

        if (step is AccountOnboardingStep.Completed && account.OnboardingStep is AccountOnboardingStep.Completed)
        {
            await referralService.UpdateReferralProgressAsync(
                accountId,
                ReferralGoalType.OnboardingCompleted,
                1,
                cancellationToken
            );

            account.Flags |= AccountFlags.HasSeenLastOnboardingStep;
        }

        accountRepository.Update(account);
        account.OnboardingStep = step;
        account.OnboardingLastUpdatedUtc = DateTime.UtcNow;

        logger.Information("Updated onboarding step for account {AccountId} from {PreviousStep} to {NewStep}",
            accountId, account.OnboardingStep, step);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ValidateStepTransitionAsync(long accountId, AccountOnboardingStep currentStep,
        AccountOnboardingStep nextStep, CancellationToken cancellationToken = default)
    {
        if (nextStep < currentStep)
        {
            return false;
        }

        if (nextStep > currentStep + 1 && nextStep != AccountOnboardingStep.Completed)
        {
            return false;
        }

        if (nextStep == AccountOnboardingStep.Completed)
        {
            await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
            var businessRepository = unitOfWork.GetRepository<BusinessEntity>();

            var hasVerifiedBusiness = await businessRepository.GetBaseQueryable()
                .AnyAsync(b => b.OwnerAccountId == accountId && b.IsVerified, cancellationToken);

            if (!hasVerifiedBusiness && currentStep < AccountOnboardingStep.BusinessVerification)
            {
                return false;
            }
        }

        return true;
    }

    public async Task<bool> HasCompletedOnboardingAsync(long accountId, CancellationToken cancellationToken = default)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var accountRepository = unitOfWork.GetRepository<AccountEntity>();

        var account = await accountRepository.GetByIdAsync(accountId, cancellationToken);
        if (account == null)
        {
            throw new NotFoundException($"Account with ID {accountId} not found.");
        }

        return account.OnboardingStep == AccountOnboardingStep.Completed;
    }

    public async Task<bool> IsStepAccessibleAsync(long accountId, AccountOnboardingStep step,
        CancellationToken cancellationToken = default)
    {
        var currentStep = await GetCurrentStepAsync(accountId, cancellationToken);

        // Users can access their current step or any previous step
        return step <= currentStep;
    }
}