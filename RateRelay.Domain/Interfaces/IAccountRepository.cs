using RateRelay.Domain.Entities;

namespace RateRelay.Domain.Interfaces;

public interface IAccountRepository : IEntityExtendedRepository<AccountEntity>
{
    Task<AccountEntity?> GetAccountByUsernameAsync(string username);
    Task<bool> AccountExistsByUsernameAsync(string username);
}