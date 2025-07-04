using Microsoft.EntityFrameworkCore;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Exceptions;
using RateRelay.Domain.Interfaces;
using RateRelay.Domain.Interfaces.DataAccess;
using Serilog;

namespace RateRelay.Infrastructure.Services;

public class UserService(
    IUnitOfWorkFactory unitOfWorkFactory
) : IUserService
{
    public async Task<AccountEntity> GetByIdAsync(long accountId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
            var accountRepository = unitOfWork.GetRepository<AccountEntity>();

            var account = await accountRepository.GetBaseQueryable()
                .Include(a => a.Role)
                .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken: cancellationToken);

            if (account is null)
            {
                throw new AppException($"Account with ID {accountId} not found.", "AccountNotFound");
            }

            return account;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error occurred while retrieving account with ID {AccountId}", accountId);
            throw new AppException("An error occurred while retrieving the account.");
        }
    }
}