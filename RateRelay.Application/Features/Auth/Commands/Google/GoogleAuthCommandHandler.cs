using MediatR;
using RateRelay.Application.DTOs.Auth.Commands;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Interfaces;
using RateRelay.Infrastructure.DataAccess.Repositories;

namespace RateRelay.Application.Features.Auth.Commands.Google;

public class GoogleAuthCommandHandler(
    IAuthService authService,
    IUnitOfWorkFactory unitOfWorkFactory
) : IRequestHandler<GoogleAuthCommand, AuthOutputDto>
{
    public async Task<AuthOutputDto> Handle(GoogleAuthCommand request, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var accountRepository = unitOfWork.GetExtendedRepository<AccountRepository>();

        var googleUserInfo = await authService.ValidateGoogleTokenAsync(request.OAuthIdToken);

        if (googleUserInfo is null)
        {
            throw new UnauthorizedAccessException("Invalid OAuth token.");
        }

        var account = await accountRepository.GetByGoogleIdAsync(googleUserInfo.GoogleId);
        var isNewAccount = false;

        if (account is null)
        {
            var existingEmailAccount = await accountRepository.GetByEmailAsync(googleUserInfo.Email);

            if (existingEmailAccount is not null)
                throw new InvalidOperationException("Account with this email already exists.");

            if (string.IsNullOrEmpty(googleUserInfo.Name))
                throw new InvalidOperationException("Google account name is required.");

            if (string.IsNullOrEmpty(googleUserInfo.GoogleId))
                throw new InvalidOperationException("Google account ID is required.");

            if (string.IsNullOrEmpty(googleUserInfo.Email))
                throw new InvalidOperationException("Google account email is required.");

            account = new AccountEntity
            {
                GoogleId = googleUserInfo.GoogleId,
                Email = googleUserInfo.Email,
                Username = googleUserInfo.Name
            };

            await accountRepository.InsertAsync(account, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            isNewAccount = true;
        }

        var token = await authService.GenerateJwtTokenAsync(account);
        var refreshToken = await authService.GenerateRefreshTokenAsync(account);

        var response = new AuthOutputDto
        {
            AccessToken = token,
            RefreshToken = refreshToken,
            IsNewUser = isNewAccount
        };

        return response;
    }
}