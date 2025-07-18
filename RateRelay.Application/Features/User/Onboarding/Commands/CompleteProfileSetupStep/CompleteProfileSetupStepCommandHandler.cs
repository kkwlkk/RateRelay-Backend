using MediatR;
using Microsoft.EntityFrameworkCore;
using RateRelay.Application.DTOs.Onboarding.Commands.CompleteProfileSetup;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Enums;
using RateRelay.Domain.Exceptions;
using RateRelay.Domain.Interfaces;
using RateRelay.Domain.Interfaces.DataAccess;
using RateRelay.Infrastructure.Services;

namespace RateRelay.Application.Features.User.Onboarding.Commands.CompleteProfileSetupStep;

public class CompleteProfileSetupCommandHandler(
    CurrentUserContext currentUserContext,
    IUnitOfWorkFactory unitOfWorkFactory,
    IOnboardingService onboardingService,
    IUserService userService
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

        if (!string.IsNullOrWhiteSpace(request.DisplayName) && request.DisplayName != account.GoogleUsername)
        {
            var isDisplayNameTaken =
                await userService.IsDisplayNameTakenAsync(request.DisplayName, account.Id, cancellationToken);

            if (isDisplayNameTaken)
            {
                throw new AppException("Display name is already taken. Please choose a different one.",
                    "DisplayNameTaken");
            }

            accountRepository.Update(account);
            account.DisplayName = request.DisplayName;
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