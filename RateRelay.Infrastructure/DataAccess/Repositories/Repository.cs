using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Interfaces;
using RateRelay.Infrastructure.DataAccess.Context;

namespace RateRelay.Infrastructure.DataAccess.Repositories;

public class Repository<T>(RateRelayDbContext dbContext) : IRepository<T>
    where T : BaseModelEntity
{
    private readonly DbSet<T> _dbSet = dbContext.Set<T>();

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<T?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync([id], cancellationToken);
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task InsertAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    public void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public void Remove(T entity)
    {
        _dbSet.Remove(entity);
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(predicate, cancellationToken);
    }

    public async Task<IEnumerable<TResult>> QueryAsync<TResult>(
        Expression<Func<T, bool>> predicate,
        Expression<Func<T, TResult>> selector,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Where(predicate)
            .Select(selector)
            .ToListAsync(cancellationToken);
    }

    public IQueryable<T> GetBaseQueryable(bool includeDeleted = false)
    {
        var query = _dbSet.AsNoTracking();
        if (includeDeleted)
            query = query.IgnoreQueryFilters();

        return query;
    }
}