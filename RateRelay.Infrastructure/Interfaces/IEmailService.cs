using RateRelay.Domain.Entities;
using RateRelay.Domain.Enums;
using RateRelay.Infrastructure.Constants;

namespace RateRelay.Infrastructure.Interfaces;

public interface IEmailService
{
    public Task SendEmailAsync(
        EmailPreferencesFlags emailType,
        string to,
        string subject,
        string body,
        bool isHtml = true,
        string? from = null,
        string? replyTo = null);

    public Task SendEmailAsync(
        EmailPreferencesFlags emailType,
        string[] to,
        string subject,
        string body,
        bool isHtml = true,
        string? from = null,
        string? replyTo = null);

    public Task SendTemplatedEmailAsync<T>(EmailPreferencesFlags emailType, string to, string subject,
        EmailTemplate template, T model, string? from = null,
        string? replyTo = null);

    public Task SendTemplatedEmailAsync<T>(EmailPreferencesFlags emailType, string[] to, string subject,
        EmailTemplate template, T model,
        string? from = null, string? replyTo = null);

    public Task SendWelcomeEmailAsync(AccountEntity account);
    public Task SendBusinessVerificationIntroEmailAsync(AccountEntity account, int daysFromRegistration);
    public Task SendIncompleteVerificationEmailAsync(AccountEntity account, string businessName, DateTime removedAt);
}