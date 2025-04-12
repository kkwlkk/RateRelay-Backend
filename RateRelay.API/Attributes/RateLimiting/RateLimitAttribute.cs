namespace RateRelay.API.Attributes.RateLimiting;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RateLimitAttribute(int limit, int periodInSeconds) : Attribute
{
    public int Limit { get; set; } = limit;
    public int PeriodInSeconds { get; set; } = periodInSeconds;

    public RateLimitAttribute() : this(0, 0)
    {
        // Default values get overridden by appsettings configuration (RateLimit:DefaultLimit)
    }
}