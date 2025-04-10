using RateRelay.Domain.Entities;

namespace RateRelay.Domain.Interfaces;

public interface IAccountRepository : IEntityExtendedRepository<AccountEntity>
{
    Task<AccountEntity?> GetByUsernameAsync(string username);
    Task<bool> ExistsByUsernameAsync(string username);
}