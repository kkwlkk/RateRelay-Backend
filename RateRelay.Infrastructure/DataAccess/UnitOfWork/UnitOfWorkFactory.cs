using Microsoft.EntityFrameworkCore;
using RateRelay.Domain.Interfaces;
using RateRelay.Infrastructure.DataAccess.Context;
using Serilog;

namespace RateRelay.Infrastructure.DataAccess.UnitOfWork;

public class UnitOfWorkFactory(
    IDbContextFactory<RateRelayDbContext> contextFactory,
    ILogger logger)
    : IUnitOfWorkFactory
{
    public async Task<IUnitOfWork> CreateAsync()
    {
        var dbContext = await contextFactory.CreateDbContextAsync();

        return new UnitOfWork(dbContext, logger);
    }

    public IUnitOfWork Create()
    {
        var dbContext = contextFactory.CreateDbContext();

        return new UnitOfWork(dbContext, logger);
    }
}