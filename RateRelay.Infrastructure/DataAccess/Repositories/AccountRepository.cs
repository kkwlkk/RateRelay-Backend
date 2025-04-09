using Microsoft.EntityFrameworkCore;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Interfaces;
using RateRelay.Infrastructure.DataAccess.Context;

namespace RateRelay.Infrastructure.DataAccess.Repositories;

public class AccountRepository(RateRelayDbContext dbContext) : Repository<AccountEntity>(dbContext), IAccountRepository
{
    private readonly RateRelayDbContext _dbContext = dbContext;

    public async Task<AccountEntity?> GetAccountByUsernameAsync(string username)
    {
        return await _dbContext.Set<AccountEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Username == username);
    }

    public Task<bool> AccountExistsByUsernameAsync(string username)
    {
        return _dbContext.Set<AccountEntity>()
            .AsNoTracking()
            .AnyAsync(x => x.Username == username);
    }
}