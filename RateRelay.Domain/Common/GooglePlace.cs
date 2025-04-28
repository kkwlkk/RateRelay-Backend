using Newtonsoft.Json;

namespace RateRelay.Domain.Common
{
    public class GooglePlace
    {
        [JsonProperty("id")]
        public string PlaceId { get; set; }

        [JsonProperty("displayName")]
        public DisplayName DisplayName { get; set; }

        [JsonProperty("googleMapsUri")]
        public string Url { get; set; }

        [JsonIgnore]
        public string Cid => ExtractCidFromUrl(Url);

        [JsonProperty("currentOpeningHours")]
        public OpeningHours CurrentOpeningHours { get; set; }

        private static string ExtractCidFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return string.Empty;

            const string cidPrefix = "cid=";
            var cidIndex = url.IndexOf(cidPrefix, StringComparison.Ordinal);
            if (cidIndex < 0) return string.Empty;
            var startIndex = cidIndex + cidPrefix.Length;
            var endIndex = url.IndexOf('&', startIndex);

            return endIndex < 0 ? url[startIndex..] : url.Substring(startIndex, endIndex - startIndex);
        }
    }

    public class OpeningHours
    {
        [JsonProperty("openNow")]
        public bool OpenNow { get; set; }

        [JsonProperty("periods")]
        public List<OpeningPeriod> Periods { get; set; }

        [JsonIgnore]
        public Dictionary<DayOfWeek, BusinessHours>? BusinessHoursByDay =>
            Periods.GroupBy(p => (DayOfWeek)p.Open.Day)
                .ToDictionary(
                    g => g.Key,
                    g => new BusinessHours(
                        new TimeSpan(g.First().Open.Hour, g.First().Open.Minute, 0),
                        new TimeSpan(g.First().Close.Hour, g.First().Close.Minute, 0)
                    )
                );
    }

    public class OpeningPeriod
    {
        [JsonProperty("open")]
        public TimeInfo Open { get; set; }

        [JsonProperty("close")]
        public TimeInfo Close { get; set; }
    }

    public class TimeInfo
    {
        [JsonProperty("day")]
        public int Day { get; set; }

        [JsonProperty("hour")]
        public int Hour { get; set; }

        [JsonProperty("minute")]
        public int Minute { get; set; }

        [JsonProperty("date")]
        public DateInfo Date { get; set; }
    }

    public class DateInfo
    {
        [JsonProperty("year")]
        public int Year { get; set; }

        [JsonProperty("month")]
        public int Month { get; set; }

        [JsonProperty("day")]
        public int Day { get; set; }
    }

    public class BusinessHours(TimeSpan openTime, TimeSpan closeTime)
    {
        public TimeSpan OpenTime { get; } = openTime;
        public TimeSpan CloseTime { get; } = closeTime;

        public override string ToString()
        {
            return $"{FormatTime(OpenTime)}–{FormatTime(CloseTime)}";
        }

        private string FormatTime(TimeSpan time)
        {
            return $"{time.Hours:D2}:{time.Minutes:D2}";
        }
    }

    public class DisplayName
    {
        public string Text { get; set; }
        public string LanguageCode { get; set; }
    }
}