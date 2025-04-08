namespace RateRelay.Domain.Interfaces;

public interface IUnitOfWorkFactory
{
    Task<IUnitOfWork> CreateAsync();
    IUnitOfWork Create();
}