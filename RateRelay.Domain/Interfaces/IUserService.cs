using RateRelay.Domain.Entities;

namespace RateRelay.Domain.Interfaces;

public interface IUserService
{
    Task<AccountEntity> GetFullAccountByIdAsync(long accountId);
}