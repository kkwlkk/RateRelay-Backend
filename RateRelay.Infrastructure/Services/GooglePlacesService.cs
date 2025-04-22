using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RateRelay.Domain.Common;
using RateRelay.Domain.Interfaces;
using RateRelay.Domain.Interfaces.Services;
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
            var endpoint = $"details/json?place_id={placeId}&fields=place_id,name,current_opening_hours,url&key={_apiKey}";
            var response = await httpClient.GetAsync(endpoint, cancellationToken);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var apiResponse = JsonConvert.DeserializeObject<GooglePlaceApiResponse>(content);

            if (apiResponse?.Status == "OK") return apiResponse.Result;
            logger.LogWarning("Google Places API returned non-OK status: {Status}, Error: {Error}",
                apiResponse?.Status, apiResponse?.ErrorMessage);
            return null;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Error fetching place details for place ID {PlaceId}", placeId);
            throw;
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

    private class GooglePlaceApiResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("error_message")]
        public string ErrorMessage { get; set; }

        [JsonProperty("result")]
        public GooglePlace Result { get; set; }
    }
}