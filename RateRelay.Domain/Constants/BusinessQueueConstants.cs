namespace RateRelay.Domain.Constants;

public static class BusinessQueueConstants
{
    public const int BusinessLockTimeoutInMinutes = 10;
    public const int SkippedBusinessCacheTimeoutInMinutes = 12 * 60; // 12 hours,
    public const int BoostedPriorityThreshold = 1;
}