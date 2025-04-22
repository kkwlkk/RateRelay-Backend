using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Interfaces;
using RateRelay.Domain.Interfaces.DataAccess;
using Serilog;

namespace RateRelay.Infrastructure.Services;

public class UserService(
    IUnitOfWorkFactory unitOfWorkFactory,
    IMapper mapper
) : IUserService
{
    public async Task<AccountEntity> GetFullAccountByIdAsync(long accountId,
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
                throw new Exception($"Account with ID {accountId} not found.");
            }

            return account;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving account with ID {AccountId}", accountId);
            throw;
        }
    }
}