namespace RateRelay.Domain.Enums;

[Flags]
public enum EmailPreferencesFlags
{
    None = 0,

    MarketingEmails = 1 << 0,

    ProductUpdates = 1 << 1,

    AccountNotifications = 1 << 2,

    SystemAnnouncements = 1 << 3,
    
    All = MarketingEmails | ProductUpdates | AccountNotifications | SystemAnnouncements,
    Essential = AccountNotifications | SystemAnnouncements
}