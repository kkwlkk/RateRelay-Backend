using RateRelay.Domain.Entities;

namespace RateRelay.Domain.Interfaces;

public interface IAccountRepository : IEntityExtendedRepository<AccountEntity>
{
    Task<AccountEntity?> GetByUsernameAsync(string username);
    Task<AccountEntity?> GetByEmailAsync(string email);
    Task<AccountEntity?> GetByGoogleIdAsync(string googleId);
    Task<bool> ExistsByUsernameAsync(string username);
    Task<bool> ExistsByEmailAsync(string email);
    Task<bool> ExistsByGoogleIdAsync(string googleId);
}