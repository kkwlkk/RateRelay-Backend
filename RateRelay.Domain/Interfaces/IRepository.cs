using System.Linq.Expressions;
using RateRelay.Domain.Entities;

namespace RateRelay.Domain.Interfaces;

public interface IRepository<T> where T : BaseModelEntity
{
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task InsertAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Remove(T entity);
    Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<IEnumerable<TResult>> QueryAsync<TResult>(
        Expression<Func<T, bool>> predicate,
        Expression<Func<T, TResult>> selector,
        CancellationToken cancellationToken = default);
    public IQueryable<T> GetBaseQueryable(bool includeDeleted = false);
}