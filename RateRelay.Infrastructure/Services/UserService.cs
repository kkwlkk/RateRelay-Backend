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

    public async Task<AccountBanEntity?> HasActiveBanAsync(long accountId,
        CancellationToken cancellationToken = default)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var accountBanRepository = unitOfWork.GetRepository<AccountBanEntity>();

        return await accountBanRepository.GetBaseQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.AccountId == accountId &&
                                      (b.ExpiresAtUtc == null || b.ExpiresAtUtc > DateTime.UtcNow),
                cancellationToken);
    }

    public async Task BanAccountAsync(long accountId, string reason, DateTime? expiresAtUtc = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
            var accountBanRepository = unitOfWork.GetRepository<AccountBanEntity>();

            var existingBan = await accountBanRepository.GetBaseQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.AccountId == accountId && b.IsActive, cancellationToken);

            if (existingBan is not null)
            {
                throw new AppException($"Account with ID {accountId} already has an active ban.",
                    "AccountAlreadyBanned");
            }

            var newBan = new AccountBanEntity
            {
                AccountId = accountId,
                Reason = reason,
                ExpiresAtUtc = expiresAtUtc
            };

            await accountBanRepository.InsertAsync(newBan, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error occurred while banning account with ID {AccountId}", accountId);
            throw new AppException("An error occurred while banning the account.");
        }
    }
}