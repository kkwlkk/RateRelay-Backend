using Microsoft.EntityFrameworkCore;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Enums;
using RateRelay.Domain.Interfaces;
using Serilog;

namespace RateRelay.Infrastructure.Services;

public class PointService(
    IUnitOfWorkFactory unitOfWorkFactory,
    ILogger logger
) : IPointService
{
    public async Task<int> GetPointBalanceAsync(long accountId, CancellationToken cancellationToken = default)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var accountRepository = unitOfWork.GetRepository<AccountEntity>();
        var account = await accountRepository.GetByIdAsync(accountId, cancellationToken);
        if (account is null)
        {
            throw new ArgumentException($"Account with ID {accountId} not found.");
        }

        return account.PointBalance;
    }

    public async Task<bool> AddPointsAsync(long accountId, int amount,
        PointTransactionType transactionType = PointTransactionType.System,
        string? description = null, CancellationToken cancellationToken = default)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var accountRepository = unitOfWork.GetRepository<AccountEntity>();
        var account = await accountRepository.GetByIdAsync(accountId, cancellationToken);

        if (account is null)
        {
            throw new ArgumentException($"Account with ID {accountId} not found.");
        }

        if (amount <= 0)
        {
            throw new ArgumentException("Amount must be greater than zero.");
        }

        try
        {
            await unitOfWork.BeginTransactionAsync(cancellationToken: cancellationToken);
            accountRepository.Update(account);
            account.PointBalance += amount;
            var pointTransactionRepository = unitOfWork.GetRepository<PointTransactionEntity>();
            var pointTransaction = new PointTransactionEntity
            {
                AccountId = accountId,
                Amount = amount,
                TransactionType = transactionType,
                Description = description
            };
            await pointTransactionRepository.InsertAsync(pointTransaction, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            logger.Information("Added {Amount} points to account {AccountId} for {TransactionType} transaction.",
                amount, accountId, transactionType);

            return true;
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.Error(ex, "Failed to add points to account {AccountId}: {Message}", accountId, ex.Message);
            throw;
        }
    }

    public async Task<bool> DeductPointsAsync(long accountId, int amount,
        PointTransactionType transactionType = PointTransactionType.System,
        string? description = null, CancellationToken cancellationToken = default)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var accountRepository = unitOfWork.GetRepository<AccountEntity>();
        var account = await accountRepository.GetByIdAsync(accountId, cancellationToken);

        if (account is null)
        {
            throw new ArgumentException($"Account with ID {accountId} not found.");
        }

        if (amount <= 0)
        {
            throw new ArgumentException("Amount must be greater than zero.");
        }

        try
        {
            await unitOfWork.BeginTransactionAsync(cancellationToken: cancellationToken);
            accountRepository.Update(account);
            account.PointBalance -= amount;
            var pointTransactionRepository = unitOfWork.GetRepository<PointTransactionEntity>();
            var pointTransaction = new PointTransactionEntity
            {
                AccountId = accountId,
                Amount = -amount,
                TransactionType = transactionType,
                Description = description
            };
            await pointTransactionRepository.InsertAsync(pointTransaction, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            logger.Information("Deducted {Amount} points from account {AccountId} for {TransactionType} transaction.",
                amount, accountId, transactionType);

            return true;
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.Error(ex, "Failed to deduct points from account {AccountId}: {Message}", accountId, ex.Message);
            throw;
        }
    }

    public async Task<bool> HasEnoughPointsAsync(long accountId, int amount,
        CancellationToken cancellationToken = default)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var accountRepository = unitOfWork.GetRepository<AccountEntity>();
        var account = await accountRepository.GetByIdAsync(accountId, cancellationToken);

        if (account is null)
        {
            throw new ArgumentException($"Account with ID {accountId} not found.");
        }

        return account.PointBalance >= amount;
    }

    public async Task<IEnumerable<PointTransactionEntity>> GetPointTransactionsAsync(long accountId,
        CancellationToken cancellationToken = default)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var pointTransactionRepository = unitOfWork.GetRepository<PointTransactionEntity>();
        var transactions = await pointTransactionRepository.GetBaseQueryable()
            .Where(t => t.AccountId == accountId)
            .OrderByDescending(t => t.TransactionDateUtc)
            .ToListAsync(cancellationToken: cancellationToken);

        return transactions;
    }
}