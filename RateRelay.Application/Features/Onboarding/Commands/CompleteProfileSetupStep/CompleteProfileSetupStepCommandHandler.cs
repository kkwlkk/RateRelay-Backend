using MediatR;
using Microsoft.EntityFrameworkCore;
using RateRelay.Application.DTOs.Onboarding.Commands.CompleteProfileSetup;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Enums;
using RateRelay.Domain.Interfaces;
using RateRelay.Domain.Interfaces.DataAccess;
using RateRelay.Infrastructure.Services;

namespace RateRelay.Application.Features.Onboarding.Commands.CompleteProfileSetupStep;

public class CompleteProfileSetupCommandHandler(
    CurrentUserContext currentUserContext,
    IUnitOfWorkFactory unitOfWorkFactory,
    IOnboardingService onboardingService
) : IRequestHandler<CompleteProfileSetupStepCommand, CompleteProfileSetupOutputDto>
{
    public async Task<CompleteProfileSetupOutputDto> Handle(CompleteProfileSetupStepCommand request,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var accountRepository = unitOfWork.GetRepository<AccountEntity>();

        var account = await accountRepository.GetByIdAsync(currentUserContext.AccountId, cancellationToken);
        if (account == null)
        {
            throw new KeyNotFoundException("Account not found");
        }

        if (!string.IsNullOrWhiteSpace(request.DisplayName) && request.DisplayName != account.Username)
        {
            var isUsernameTaken = await accountRepository.GetBaseQueryable()
                .AnyAsync(a => a.Id != account.Id && a.Username == request.DisplayName, cancellationToken);

            if (isUsernameTaken)
            {
                throw new InvalidOperationException("Username is already taken");
            }

            accountRepository.Update(account);
            account.Username = request.DisplayName;
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        await onboardingService.UpdateStepAsync(
            currentUserContext.AccountId,
            AccountOnboardingStep.Completed,
            cancellationToken);

        return new CompleteProfileSetupOutputDto
        {
            NextStep = AccountOnboardingStep.Completed,
        };
    }
}