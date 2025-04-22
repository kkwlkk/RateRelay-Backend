using RateRelay.Domain.Common;

namespace RateRelay.Domain.Interfaces;

public interface IGooglePlacesService
{
    Task<GooglePlace?> GetPlaceDetailsAsync(string placeId, CancellationToken cancellationToken = default);
}