namespace RateRelay.Domain.Interfaces.DataAccess;

public interface IUnitOfWorkFactory
{
    Task<IUnitOfWork> CreateAsync();
    IUnitOfWork Create();
}