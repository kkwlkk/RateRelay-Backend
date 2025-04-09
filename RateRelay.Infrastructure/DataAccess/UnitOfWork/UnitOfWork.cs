using System.Collections.Concurrent;
using System.Data;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Interfaces;
using RateRelay.Infrastructure.DataAccess.Context;
using RateRelay.Infrastructure.DataAccess.Repositories;
using Serilog;

namespace RateRelay.Infrastructure.DataAccess.UnitOfWork;

internal class UnitOfWork(RateRelayDbContext dbContext, ILogger logger) : IUnitOfWork
{
    private readonly RateRelayDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ConcurrentDictionary<Type, object> _repositories = new();
    private IDbContextTransaction? _currentTransaction;

    public IRepository<TEntity> GetRepository<TEntity>() where TEntity : BaseEntity
    {
        return (IRepository<TEntity>)_repositories.GetOrAdd(typeof(TEntity), _ => new Repository<TEntity>(_dbContext));
    }

    public TRepository GetExtendedRepository<TRepository>() where TRepository : class
    {
        return (TRepository)_repositories.GetOrAdd(typeof(TRepository), _ =>
        {
            var repositoryInterfaceType = typeof(TRepository);

            var repositoryType = Assembly.GetExecutingAssembly()
                .GetTypes()
                .FirstOrDefault(t =>
                    !t.IsInterface &&
                    !t.IsAbstract &&
                    repositoryInterfaceType.IsAssignableFrom(t));

            if (repositoryType is null)
            {
                throw new NotSupportedException(
                    $"Implementation for repository type {repositoryInterfaceType.Name} was not found");
            }

            return Activator.CreateInstance(repositoryType, _dbContext) ??
                   throw new InvalidOperationException($"Failed to create instance of {repositoryType.Name}");
        });
    }

    public async Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is not null)
        {
            throw new InvalidOperationException("A transaction is already in progress.");
        }

        try
        {
            _currentTransaction = await _dbContext.Database.BeginTransactionAsync(isolationLevel, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "An error occurred while starting a transaction.");
            throw;
        }
    }


    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null)
        {
            throw new InvalidOperationException("No active transaction to commit.");
        }

        try
        {
            await SaveChangesAsync(cancellationToken);
            await _currentTransaction.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "An error occurred while committing the transaction.");
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null)
        {
            return;
        }

        try
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "An error occurred while rolling back the transaction.");
            throw;
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    private async Task DisposeTransactionAsync()
    {
        if (_currentTransaction is not null)
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "An error occurred while saving changes to the database.");
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_currentTransaction is not null)
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }

        await _dbContext.DisposeAsync();
    }
}