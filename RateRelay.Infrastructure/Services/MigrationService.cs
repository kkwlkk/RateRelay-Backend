using System.Reflection;
using RateRelay.Domain.Interfaces;
using DbUp;
using DbUp.Engine.Output;
using Microsoft.Extensions.Options;
using RateRelay.Infrastructure.Configuration;
using Serilog;

namespace RateRelay.Infrastructure.Services;

public class MigrationService : IMigrationService
{
    private readonly string _connectionString;
    private readonly ILogger _logger;
    private readonly TimeSpan _executionTimeout;

    public MigrationService(IOptions<DatabaseOptions> options, ILogger logger)
    {
        var databaseOptions = options.Value;

        _connectionString = $"{databaseOptions.ConnectionString}Password={databaseOptions.Password}";
        _executionTimeout = databaseOptions.MigrationTimeout;
        _logger = logger;
    }

    public Task<bool> UpdateDatabaseAsync(CancellationToken cancellationToken = default)
    {
        _logger.Information("Starting database migration process...");

        var upgrader = DeployChanges.To
            .MySqlDatabase(_connectionString)
            .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
            .LogTo(new SerilogUpgradeLog(_logger))
            .WithTransactionPerScript()
            .WithExecutionTimeout(_executionTimeout)
            .Build();

        if (!upgrader.IsUpgradeRequired())
        {
            _logger.Information("Database is up to date.");
            return Task.FromResult(true);
        }

        _logger.Warning("Database needs updates. Preparing to execute migration scripts...");

        var scriptsToExecute = upgrader.GetScriptsToExecute();

        _logger.Warning("Number of scripts to execute: {ScriptsCount}", scriptsToExecute.Count);

        var upgradeResult = upgrader.PerformUpgrade();

        if (upgradeResult.Successful)
        {
            _logger.Information("Database updated successfully.");
            return Task.FromResult(true);
        }

        _logger.Error("Database update failed. Error: {Error}", upgradeResult.Error);
        return Task.FromResult(false);
    }

    private class SerilogUpgradeLog(ILogger logger) : IUpgradeLog
    {
        public void LogTrace(string format, params object[] args)
        {
            logger.Verbose(format, args);
        }

        public void LogDebug(string format, params object[] args)
        {
            logger.Debug(format, args);
        }

        public void LogInformation(string format, params object[] args)
        {
            logger.Information(format, args);
        }

        public void LogWarning(string format, params object[] args)
        {
            logger.Warning(format, args);
        }

        public void LogError(string format, params object[] args)
        {
            logger.Error(format, args);
        }

        public void LogError(Exception ex, string format, params object[] args)
        {
            logger.Error(ex, format, args);
        }
    }
}