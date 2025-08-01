using Serilog.Core;
using Serilog.Events;

namespace RateRelay.Infrastructure.Logging.Enrichers;

public class PrefixFormatterEnricher(Dictionary<string, string> prefixColorMapping) : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        string formattedPrefixWithColor;
        string formattedPrefixPlain;

        if (logEvent.Properties.TryGetValue("Prefix", out var prefix))
        {
            var prefixString = prefix.ToString().Trim('"');
            var ansiColorCode = prefixColorMapping.GetValueOrDefault(prefixString, AnsiColor.Gray);

            formattedPrefixWithColor = $" \u001b[{ansiColorCode}m[{prefixString}]\u001b[0m";
            formattedPrefixPlain = $" [{prefixString}]";
        }
        else
        {
            formattedPrefixWithColor = "";
            formattedPrefixPlain = "";
        }

        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("FormattedPrefixWithColor",
            formattedPrefixWithColor));
        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("FormattedPrefixPlain", formattedPrefixPlain));
    }
}