using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RateRelay.Domain.Interfaces;
using RateRelay.Infrastructure.Configuration;
using RateRelay.Infrastructure.Services;

namespace RateRelay.Infrastructure.Extensions;

public static class GoogleApisExtensions
{
    public static void AddGooglePlacesService(this IServiceCollection services, IConfiguration configuration)
    {
        var googleApisOptions = configuration.GetSection(GoogleApisOptions.SectionName).Get<GoogleApisOptions>();
        if (googleApisOptions == null)
        {
            throw new ArgumentNullException(nameof(googleApisOptions), "Google API options are not configured.");
        }

        services.AddHttpClient<IGooglePlacesService, GooglePlacesService>(client =>
        {
            client.BaseAddress = new Uri("https://places.googleapis.com/v1/places/");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("User-Agent", "RateRelay");
        });
    }
}