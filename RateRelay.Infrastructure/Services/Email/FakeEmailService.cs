using RateRelay.Domain.Entities;
using RateRelay.Domain.Enums;
using RateRelay.Infrastructure.Constants;
using RateRelay.Infrastructure.Interfaces;

namespace RateRelay.Infrastructure.Services.Email;

public class FakeEmailService : IEmailService
{
    public Task SendEmailAsync(EmailPreferencesFlags emailType, string to, string subject, string body, bool isHtml = true,
        string? from = null, string? replyTo = null)
    {
        return Task.CompletedTask;
    }

    public Task SendEmailAsync(EmailPreferencesFlags emailType, string[] to, string subject, string body, bool isHtml = true,
        string? from = null, string? replyTo = null)
    {
        return Task.CompletedTask;
    }

    public Task SendTemplatedEmailAsync<T>(EmailPreferencesFlags emailType, string to, string subject, EmailTemplate template,
        T model, string? from = null, string? replyTo = null)
    {
        return Task.CompletedTask;
    }

    public Task SendTemplatedEmailAsync<T>(EmailPreferencesFlags emailType, string[] to, string subject, EmailTemplate template,
        T model, string? from = null, string? replyTo = null)
    {
        return Task.CompletedTask;
    }

    public Task SendWelcomeEmailAsync(AccountEntity account)
    {
        return Task.CompletedTask;
    }

    public Task SendBusinessVerificationIntroEmailAsync(AccountEntity account, int daysFromRegistration)
    {
        return Task.CompletedTask;
    }

    public Task SendIncompleteVerificationEmailAsync(AccountEntity account, string businessName, DateTime removedAt)
    {
        return Task.CompletedTask;
    }
}