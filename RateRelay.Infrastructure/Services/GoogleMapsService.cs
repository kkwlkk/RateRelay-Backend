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
}