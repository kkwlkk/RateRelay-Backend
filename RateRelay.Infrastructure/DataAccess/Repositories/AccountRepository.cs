using Microsoft.EntityFrameworkCore;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Interfaces.DataAccess;
using RateRelay.Infrastructure.DataAccess.Context;

namespace RateRelay.Infrastructure.DataAccess.Repositories;

public class AccountRepository(RateRelayDbContext dbContext) : Repository<AccountEntity>(dbContext), IAccountRepository
{
    private readonly RateRelayDbContext _dbContext = dbContext;

    public async Task<AccountEntity?> GetByUsernameAsync(string username)
    {
        return await _dbContext.Set<AccountEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.GoogleUsername == username);
    }

    public Task<bool> ExistsByUsernameAsync(string username)
    {
        return _dbContext.Set<AccountEntity>()
            .AsNoTracking()
            .AnyAsync(x => x.GoogleUsername == username);
    }

    /// <summary>
    /// Retrieves an account by refresh token. If the refresh token is expired or invalid, null is returned.
    /// </summary>
    public async Task<AccountEntity?> GetByRefreshTokenAsync(string refreshToken)
    {
        return await _dbContext.Set<RefreshTokenEntity>()
            .AsNoTracking()
            .Where(rt => rt.Token == refreshToken && rt.ExpirationDate > DateTime.UtcNow)
            .Include(rt => rt.Account)
            .Select(rt => rt.Account)
            .FirstOrDefaultAsync();
    }

    public async Task<AccountEntity?> GetByEmailAsync(string email)
    {
        return await _dbContext.Set<AccountEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email == email);
    }

    public Task<bool> ExistsByEmailAsync(string email)
    {
        return _dbContext.Set<AccountEntity>()
            .AsNoTracking()
            .AnyAsync(x => x.Email == email);
    }

    public Task<bool> ExistsByGoogleIdAsync(string googleId)
    {
        return _dbContext.Set<AccountEntity>()
            .AsNoTracking()
            .AnyAsync(x => x.GoogleId == googleId);
    }

    public async Task<AccountEntity?> GetByGoogleIdAsync(string googleId)
    {
        return await _dbContext.Set<AccountEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.GoogleId == googleId);
    }
}