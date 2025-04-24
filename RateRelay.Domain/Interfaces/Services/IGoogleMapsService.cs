namespace RateRelay.Domain.Interfaces;

public interface IGoogleMapsService
{
    string GenerateMapUrlFromCid(string cid);
}