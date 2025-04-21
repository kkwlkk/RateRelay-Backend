using Newtonsoft.Json;

namespace RateRelay.Domain.Common
{
    public class GooglePlace
    {
        [JsonProperty("place_id")]
        public string PlaceId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("current_opening_hours")]
        public OpeningHours CurrentOpeningHours { get; set; }
    }

    public class OpeningHours
    {
        [JsonProperty("open_now")]
        public bool OpenNow { get; set; }

        [JsonProperty("periods")]
        public List<OpeningPeriod> Periods { get; set; }

        [JsonIgnore]
        public Dictionary<DayOfWeek, BusinessHours>? BusinessHoursByDay =>
            Periods.GroupBy(p => (DayOfWeek)p.Open.Day)
                .ToDictionary(
                    g => g.Key,
                    g => new BusinessHours(
                        TimeSpan.ParseExact(g.First().Open.Time, "hhmm", null),
                        TimeSpan.ParseExact(g.First().Close.Time, "hhmm", null)
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

        [JsonProperty("time")]
        public string Time { get; set; }
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
}