namespace RateRelay.Domain.Interfaces;

public interface IMigrationService
{
    Task<bool> UpdateDatabaseAsync(CancellationToken cancellationToken = default);
}