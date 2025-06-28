using MediatR;
using RateRelay.Application.DTOs.Auth.Commands;
using RateRelay.Domain.Interfaces;
using RateRelay.Domain.Interfaces.DataAccess;
using RateRelay.Infrastructure.DataAccess.Repositories;

namespace RateRelay.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler(
    IAuthService authService,
    IUnitOfWorkFactory unitOfWorkFactory
) : IRequestHandler<RefreshTokenCommand, RefreshTokenOutputDto>
{
    public async Task<RefreshTokenOutputDto> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken
    )
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var accountRepository = unitOfWork.GetExtendedRepository<AccountRepository>();

        var account = await accountRepository.GetByRefreshTokenAsync(request.RefreshToken);
        
        if (account is null)
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        await authService.InvalidateRefreshTokenAsync(request.RefreshToken);
        var newAccessToken = await authService.GenerateJwtTokenAsync(account);
        var newRefreshToken = await authService.GenerateRefreshTokenAsync(account);

        var response = new RefreshTokenOutputDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
        };

        return response;
    }
}