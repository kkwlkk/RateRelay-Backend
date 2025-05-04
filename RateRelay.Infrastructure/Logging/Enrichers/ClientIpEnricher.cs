using Serilog.Core;
using Serilog.Events;

namespace RateRelay.Infrastructure.Logging.Enrichers;

public class ClientIpEnricher : ILogEventEnricher
{
    private const string ClientIpPropertyName = "ClientIP";

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (!logEvent.Properties.TryGetValue(ClientIpPropertyName, out var existingProperty))
        {
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(ClientIpPropertyName, ""));
            return;
        }

        if (existingProperty is ScalarValue { Value: string value })
        {
            logEvent.AddOrUpdateProperty(string.IsNullOrEmpty(value)
                ? propertyFactory.CreateProperty(ClientIpPropertyName, "")
                : propertyFactory.CreateProperty(ClientIpPropertyName, $" [{value}]"));
        }
    }
}