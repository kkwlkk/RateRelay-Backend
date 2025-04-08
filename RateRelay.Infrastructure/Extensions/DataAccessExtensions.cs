using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RateRelay.Domain.Interfaces;
using RateRelay.Infrastructure.Configuration;
using RateRelay.Infrastructure.DataAccess.Context;
using RateRelay.Infrastructure.DataAccess.Repositories;
using RateRelay.Infrastructure.DataAccess.UnitOfWork;
using RateRelay.Infrastructure.Services;

namespace RateRelay.Infrastructure.Extensions;

public static class DataAccessExtensions
{
    public static void AddRateRelayDatabase(this IServiceCollection services,IConfiguration configuration)
    {
        var connectionString = configuration.GetDatabaseConnectionString();

        services.AddDbContextFactory<RateRelayDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
                builder => { builder.EnableRetryOnFailure(5); }));

        services.AddSingleton<MigrationService>();
        services.AddSingleton<IUnitOfWorkFactory, UnitOfWorkFactory>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
    }

    private static string GetDatabaseConnectionString(this IConfiguration configuration)
    {
        var databaseOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>();

        if (databaseOptions is null)
            throw new ArgumentNullException(nameof(databaseOptions));

        return $"{databaseOptions.ConnectionString};password={databaseOptions.Password}";
    }
}