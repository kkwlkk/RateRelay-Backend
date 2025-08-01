namespace RateRelay.Infrastructure.Hangfire;

[AttributeUsage(AttributeTargets.Class)]
public class HangfireRecurringJobAttribute(string recurringJobId, string cronExpression) : Attribute
{
    public string RecurringJobId { get; } = recurringJobId ?? throw new ArgumentNullException(nameof(recurringJobId));
    public string CronExpression { get; } = cronExpression ?? throw new ArgumentNullException(nameof(cronExpression));
    internal TimeZoneInfo TimeZone { get; set; } = TimeZoneInfo.Utc;
}