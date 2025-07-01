using System.ComponentModel;

namespace RateRelay.Domain.Constants;

public static class TicketConstants
{
    [Description("Cooldown period in seconds for creating tickets after a previous one (in seconds)")]
    public const int CooldownPeriodSeconds = 60 * 60;
}