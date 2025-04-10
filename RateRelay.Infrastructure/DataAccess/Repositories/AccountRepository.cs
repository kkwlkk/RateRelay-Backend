using Microsoft.EntityFrameworkCore;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Interfaces;
using RateRelay.Infrastructure.DataAccess.Context;

namespace RateRelay.Infrastructure.DataAccess.Repositories;

public class AccountRepository(RateRelayDbContext dbContext) : Repository<AccountEntity>(dbContext), IAccountRepository
{
    private readonly RateRelayDbContext _dbContext = dbContext;

    public async Task<AccountEntity?> GetByUsernameAsync(string username)
    {
        return await _dbContext.Set<AccountEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Username == username);
    }

    public Task<bool> ExistsByUsernameAsync(string username)
    {
        return _dbContext.Set<AccountEntity>()
            .AsNoTracking()
            .AnyAsync(x => x.Username == username);
    }

    /// <summary>
    /// Retrieves an account by refresh token. If the refresh token is expired or invalid, null is returned.
    /// </summary>
    public async Task<AccountEntity?> GetByRefreshTokenAsync(string refreshToken)
    {
        return await _dbContext.Set<RefreshTokenEntity>()
            .AsNoTracking()
            .Where(rt => rt.Token == refreshToken && rt.ExpirationDate > DateTime.UtcNow)
            .Select(rt => rt.Account)
            .FirstOrDefaultAsync();
    }
}