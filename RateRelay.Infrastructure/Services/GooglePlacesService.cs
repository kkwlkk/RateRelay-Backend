using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RateRelay.Domain.Common;
using RateRelay.Domain.Exceptions;
using RateRelay.Domain.Interfaces;
using RateRelay.Infrastructure.Configuration;

namespace RateRelay.Infrastructure.Services;

public class GooglePlacesService(
    HttpClient httpClient,
    IOptions<GoogleApisOptions> googleApisOptions,
    ILogger<GooglePlacesService> logger)
    : IGooglePlacesService
{
    private readonly string _apiKey = googleApisOptions.Value.ApiKey;

    public async Task<GooglePlace?> GetPlaceDetailsAsync(string placeId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(placeId))
        {
            throw new ArgumentException("Place ID cannot be null or empty", nameof(placeId));
        }

        try
        {
            var endpoint =
                $"{placeId}?fields=id,displayName,formattedAddress,googleMapsUri,currentOpeningHours&key={_apiKey}";
            var response = await httpClient.GetAsync(endpoint, cancellationToken);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var placeDetails = JsonConvert.DeserializeObject<GooglePlace>(content);
            if (placeDetails is null)
            {
                logger.LogWarning("No place details found for place ID {PlaceId}", placeId);
                return null;
            }

            if (string.IsNullOrEmpty(placeDetails.PlaceId))
            {
                logger.LogWarning("Place ID is empty for place ID {PlaceId}", placeId);
                return null;
            }

            return placeDetails;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Error fetching place details for place ID {PlaceId}", placeId);
            throw new AppException(
                "Error while trying to fetch place details from Google Places API",
                "ERR_PLACE_DETAILS_FETCH");
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Error deserializing place details for place ID {PlaceId}", placeId);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error getting place details for place ID {PlaceId}", placeId);
            throw;
        }
    }
}