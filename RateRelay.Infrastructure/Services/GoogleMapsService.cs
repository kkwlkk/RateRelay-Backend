using RateRelay.Domain.Interfaces;

namespace RateRelay.Infrastructure.Services;

public class GoogleMapsService : IGoogleMapsService
{
    private const string GoogleMapsCidBaseUrl = "https://maps.google.com/?cid=";

    public string GenerateMapUrlFromCid(string cid)
    {
        if (string.IsNullOrWhiteSpace(cid))
        {
            throw new ArgumentException("CID cannot be null or empty.", nameof(cid));
        }

        return $"{GoogleMapsCidBaseUrl}{cid}";
    }

    public string GenerateMapReviewUrl(string placeId, string userGoogleId)
    {
        if (string.IsNullOrWhiteSpace(placeId))
        {
            throw new ArgumentException("Place ID cannot be null or empty.", nameof(placeId));
        }

        if (string.IsNullOrWhiteSpace(userGoogleId))
        {
            throw new ArgumentException("User Google ID cannot be null or empty.", nameof(userGoogleId));
        }

        return $"https://www.google.com/maps/contrib/{userGoogleId}/place/{placeId}";
    }
}