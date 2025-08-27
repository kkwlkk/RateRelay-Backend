using Microsoft.Extensions.Options;
using RateRelay.Infrastructure.Configuration;
using RateRelay.Infrastructure.Interfaces;
using MimeKit;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Enums;
using RateRelay.Domain.Interfaces.DataAccess;
using RateRelay.Infrastructure.Constants;
using RateRelay.Infrastructure.DataAccess.Repositories;

namespace RateRelay.Infrastructure.Services;

public class EmailService(
    IOptions<EmailOptions> options,
    IEmailTemplateService templateService,
    IEmailModelBuilderService builderService,
    IOptions<CompanyOptions> companyOptions,
    IUnitOfWorkFactory unitOfWorkFactory
) : IEmailService
{
    public async Task SendEmailAsync(
        EmailPreferencesFlags emailType,
        string to, string subject, string body, bool isHtml = true, string? from = null,
        string? replyTo = null)
    {
        if (!await CanSendEmailTypeAsync(to, emailType))
            return;

        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(from ?? options.Value.FromName, options.Value.Username));
        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = subject;

        var bodyBuilder = new BodyBuilder();

        if (isHtml)
        {
            bodyBuilder.HtmlBody = body;

            var htmlPart = new TextPart("html")
            {
                Text = body,
                ContentTransferEncoding = ContentEncoding.EightBit
            };
            htmlPart.ContentType.Charset = "utf-8";

            email.Body = htmlPart;
        }
        else
        {
            bodyBuilder.TextBody = body;
            email.Body = bodyBuilder.ToMessageBody();
        }

        if (!string.IsNullOrEmpty(replyTo))
        {
            email.ReplyTo.Add(MailboxAddress.Parse(replyTo));
        }

        using var client = new MailKit.Net.Smtp.SmtpClient();

        client.ServerCertificateValidationCallback = (_, _, chain, sslPolicyErrors) =>
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            return sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors &&
                   chain?.ChainStatus.All(x => x.Status == X509ChainStatusFlags.UntrustedRoot) == true;
        };

        await client.ConnectAsync(options.Value.SmtpHost, options.Value.SmtpPort,
            MailKit.Security.SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(options.Value.Username, options.Value.Password);
        await client.SendAsync(email);
        await client.DisconnectAsync(true);
    }

    public Task SendEmailAsync(EmailPreferencesFlags emailType, string[] to, string subject, string body,
        bool isHtml = true, string? from = null,
        string? replyTo = null)
    {
        var tasks = to.Select(email => SendEmailAsync(emailType, email, subject, body, isHtml, from, replyTo));
        return Task.WhenAll(tasks);
    }

    public async Task SendTemplatedEmailAsync<T>(EmailPreferencesFlags emailType, string to, string subject,
        EmailTemplate template, T model,
        string? from = null, string? replyTo = null)
    {
        var htmlContent = await templateService.RenderTemplateAsync(template.ToString(), model);
        await SendEmailAsync(emailType, to, subject, htmlContent, true, from, replyTo);
    }

    public Task SendTemplatedEmailAsync<T>(EmailPreferencesFlags emailType, string[] to, string subject,
        EmailTemplate template, T model,
        string? from = null, string? replyTo = null)
    {
        var tasks = to.Select(email =>
            SendTemplatedEmailAsync(emailType, email, subject, template, model, from, replyTo));
        return Task.WhenAll(tasks);
    }

    private async Task<bool> CanSendEmailTypeAsync(string emailAddress, EmailPreferencesFlags emailType)
    {
        await using var uow = await unitOfWorkFactory.CreateAsync();
        var accountRepository = uow.GetExtendedRepository<AccountRepository>();
        var account = await accountRepository.GetByEmailAsync(emailAddress);
        return account is not null && account.EmailPreferences.HasFlag(emailType);
    }

    public Task SendWelcomeEmailAsync(AccountEntity account)
    {
        var model = builderService.BuildWelcomeModel(
            new Models.Email.Base.User { Name = account.GoogleUsername, Email = account.Email }
        );

        var subject = $"Witaj w {companyOptions.Value.Name}!";
        return SendTemplatedEmailAsync(EmailPreferencesFlags.AccountNotifications, account.Email, subject,
            EmailTemplate.Welcome, model);
    }

    public Task SendBusinessVerificationIntroEmailAsync(AccountEntity account, int daysFromRegistration)
    {
        var model = builderService.BuildBusinessVerificationIntroModel(
            new Models.Email.Base.User { Name = account.GoogleUsername, Email = account.Email },
            daysFromRegistration
        );

        const string subject = "Dokończ weryfikację swojej firmy";
        return SendTemplatedEmailAsync(EmailPreferencesFlags.AccountNotifications, account.Email, subject,
            EmailTemplate.BusinessVerificationIntro, model);
    }

    public Task SendIncompleteVerificationEmailAsync(AccountEntity account, string businessName, DateTime removedAt)
    {
        var model = builderService.BuildIncompleteVerificationModel(
            new Models.Email.Base.User { Name = account.GoogleUsername, Email = account.Email },
            businessName,
            removedAt
        );

        const string subject = "Nie udało się zweryfikować Twojego biznesu";
        return SendTemplatedEmailAsync(EmailPreferencesFlags.AccountNotifications, account.Email, subject,
            EmailTemplate.IncompleteVerification, model);
    }
}