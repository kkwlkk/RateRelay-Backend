using System.Data;
using Microsoft.EntityFrameworkCore.Storage;
using RateRelay.Domain.Entities;

namespace RateRelay.Domain.Interfaces;

public interface IUnitOfWork : IAsyncDisposable
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    IRepository<TEntity> GetRepository<TEntity>() where TEntity : BaseEntity;
    TRepository GetExtendedRepository<TRepository>() where TRepository : class;

    Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default);

    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}