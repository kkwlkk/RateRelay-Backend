using MediatR;
using RateRelay.Application.DTOs.Auth.Commands;
using RateRelay.Domain.Interfaces;
using RateRelay.Infrastructure.DataAccess.Repositories;

namespace RateRelay.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler(
    IAuthService authService,
    IUnitOfWorkFactory unitOfWorkFactory
) : IRequestHandler<LoginCommand, LoginOutputDto>
{
    public async Task<LoginOutputDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var accountRepository = unitOfWork.GetExtendedRepository<AccountRepository>();

        var account = await accountRepository.GetByUsernameAsync(request.Username);

        if (account is null)
        {
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        var isValidPassword = authService.VerifyPassword(account.PasswordHash, request.Password);

        if (!isValidPassword)
        {
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        var token = await authService.GenerateJwtTokenAsync(account);
        var refreshToken = await authService.GenerateRefreshTokenAsync(account);

        var response = new LoginOutputDto
        {
            AccessToken = token,
            RefreshToken = refreshToken,
        };

        return response;
    }
}