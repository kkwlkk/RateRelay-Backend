using MediatR;
using RateRelay.Application.DTOs.User.Auth.Commands;
using RateRelay.Domain.Constants.ErrorCodes;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Exceptions;
using RateRelay.Domain.Interfaces;
using RateRelay.Domain.Interfaces.DataAccess;
using RateRelay.Infrastructure.DataAccess.Repositories;
using Serilog;

namespace RateRelay.Application.Features.User.Auth.Commands.Google;

public class GoogleAuthCommandHandler(
    IAuthService authService,
    IUnitOfWorkFactory unitOfWorkFactory,
    IReferralService referralService,
    ILogger logger
) : IRequestHandler<GoogleAuthCommand, AuthOutputDto>
{
    public async Task<AuthOutputDto> Handle(GoogleAuthCommand request, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var accountRepository = unitOfWork.GetExtendedRepository<AccountRepository>();

        var googleUserInfo = await authService.ValidateGoogleTokenAsync(request.OAuthIdToken);

        if (googleUserInfo is null)
        {
            throw new AppException("Invalid OAuth token.", AuthErrorCodes.InvalidToken);
        }

        var accountExistsDeleted = accountRepository.GetBaseQueryable(true)
            .Where(x => (x.GoogleId == googleUserInfo.GoogleId || x.Email == googleUserInfo.Email) && x.DateDeletedUtc != null);

        if (accountExistsDeleted.Any())
        {
            throw new AppException(
                "An account with this Google ID or email already exists, but is deleted. Please contact support to restore your account.",
                AuthErrorCodes.AccountDeleted);
        }

        var account = await accountRepository.GetByGoogleIdAsync(googleUserInfo.GoogleId);
        var isNewAccount = false;
        var isReferralLinked = false;

        if (account is null)
        {
            var existingEmailAccount = await accountRepository.GetByEmailAsync(googleUserInfo.Email);

            if (existingEmailAccount is not null)
            {
                throw new AppException(
                    "Account with this email already exists.", 
                    AuthErrorCodes.AccountExists);
            }

            if (string.IsNullOrEmpty(googleUserInfo.Name))
            {
                throw new AppException(
                    "Google account name is required.", 
                    AuthErrorCodes.MissingGoogleData);
            }

            if (string.IsNullOrEmpty(googleUserInfo.GoogleId))
            {
                throw new AppException(
                    "Google account ID is required.", 
                    AuthErrorCodes.MissingGoogleData);
            }

            if (string.IsNullOrEmpty(googleUserInfo.Email))
            {
                throw new AppException(
                    "Google account email is required.", 
                    AuthErrorCodes.MissingGoogleData);
            }

            account = new AccountEntity
            {
                GoogleId = googleUserInfo.GoogleId,
                Email = googleUserInfo.Email,
                GoogleUsername = googleUserInfo.Name
            };

            await accountRepository.InsertAsync(account, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            isNewAccount = true;

            if (!string.IsNullOrWhiteSpace(request.ReferralCode))
            {
                try
                {
                    await referralService.LinkReferralAsync(account.Id, request.ReferralCode, cancellationToken);
                    isReferralLinked = true;
                }
                catch (Exception ex)
                {
                    logger.Warning("Failed to link referral code: {Message} for AccountId: {AccountId}", ex.Message,
                        account.Id);
                    isReferralLinked = false;
                }
            }
        }

        var token = await authService.GenerateJwtTokenAsync(account);
        var refreshToken = await authService.GenerateRefreshTokenAsync(account);

        var response = new AuthOutputDto
        {
            AccessToken = token,
            RefreshToken = refreshToken,
            IsNewUser = isNewAccount,
            IsReferralLinked = isReferralLinked,
        };

        return response;
    }
}