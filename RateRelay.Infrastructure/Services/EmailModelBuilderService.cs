using Microsoft.Extensions.Options;
using RateRelay.Infrastructure.Configuration;
using RateRelay.Infrastructure.Interfaces;
using RateRelay.Infrastructure.Models.Email;
using RateRelay.Infrastructure.Models.Email.Base;

namespace RateRelay.Infrastructure.Services;

public class EmailModelBuilderService(
    IOptions<CompanyOptions> companyOptions,
    IOptions<EmailLinksOptions> emailLinksOptions)
    : IEmailModelBuilderService
{
    private readonly string _websiteUrl = companyOptions.Value.Website;
    private readonly string _dashboardUrl = $"{companyOptions.Value.Website}/dashboard";
    private readonly string _businessVerificationUrl = $"{companyOptions.Value.Website}/onboarding/business-verification";

    public BaseEmailModel BuildBaseModel(User user, string subject)
    {
        return new BaseEmailModel
        {
            Subject = subject,
            Company = new Company
            {
                Name = companyOptions.Value.Name,
                LogoUrl = companyOptions.Value.LogoUrl,
                LogoWidth = companyOptions.Value.LogoWidth,
                LogoHeight = companyOptions.Value.LogoHeight,
                Address = companyOptions.Value.Address,
                Website = companyOptions.Value.Website,
                PrivacyEmail = companyOptions.Value.Emails.PrivacyEmail,
                SocialLinks = companyOptions.Value.SocialLinks.Select(sl => new SocialLink
                {
                    Platform = sl.Platform,
                    Url = sl.Url
                }).ToList()
            },
            User = user,
            Links = new Links
            {
                Support = emailLinksOptions.Value.Support,
                Unsubscribe = emailLinksOptions.Value.Unsubscribe,
                Preferences = emailLinksOptions.Value.Preferences
            }
        };
    }

    public WelcomeEmailModel BuildWelcomeModel(User user)
    {
        var baseModel = BuildBaseModel(user, "Witamy w TrustRate!");

        return new WelcomeEmailModel
        {
            Subject = baseModel.Subject,
            Company = baseModel.Company,
            User = baseModel.User,
            Links = baseModel.Links,
            Features = [
                "Twórz i zarządzaj kampaniami recenzji",
                "Monitoruj oceny i opinie klientów w czasie rzeczywistym",
                "Generuj szczegółowe raporty i analizy",
                "Integruj z popularnymi platformami e-commerce"
            ],
            Cta = new Cta
            {
                PrimaryText = "Przejdź do panelu",
                SecondaryText = "Przewodnik szybkiego startu"
            },
            DashboardUrl = _dashboardUrl,
            SecondaryUrl = $"{_websiteUrl}/getting-started"
        };
    }

    public BusinessVerificationIntroEmailModel BuildBusinessVerificationIntroModel(User user, int daysFromRegistration)
    {
        var baseModel = BuildBaseModel(user, "Zweryfikuj swoją firmę w TrustRate");

        return new BusinessVerificationIntroEmailModel
        {
            Subject = baseModel.Subject,
            Company = baseModel.Company,
            User = baseModel.User,
            Links = baseModel.Links,
            Benefits =
            [
                "Zwiększona wiarygodność w oczach klientów",
                "Lepsza widoczność w wynikach wyszukiwania",
                "Dostęp do zaawansowanych funkcji i narzędzi"
            ],
            Reasons =
            [
                "Ułatwienie klientom znalezienia Twojej firmy",
                "Budowanie zaufania poprzez weryfikację",
                "Poprawa reputacji online"
            ],
            Cta = new Cta
            {
                PrimaryText = "Zweryfikuj teraz"
            },
            VerificationUrl = _businessVerificationUrl,
            DaysFromRegistration = daysFromRegistration
        };
    }

    public IncompleteVerificationEmailModel BuildIncompleteVerificationModel(User user, string businessName, DateTime removedAt)
    {
        var baseModel = BuildBaseModel(user, "Dokończ weryfikację swojej firmy");

        return new IncompleteVerificationEmailModel
        {
            Subject = baseModel.Subject,
            Company = baseModel.Company,
            User = baseModel.User,
            Links = baseModel.Links,
            PossibleReasons = [
                "Godziny otwarcia nie zostały jeszcze zaktualizowane",
                "Podane godziny otwarcia są niezgodne z rzeczywistymi",
                "Brak potwierdzenia zmiany godzin na wizytówce Google",
                "Brak czasu na wprowadzenie zmian",
                "Pomyłka przy edycji godzin otwarcia"
            ],
            Cta = new Cta
            {
                PrimaryText = "Rozpocznij ponownie",
                // SecondaryText = "Prześlij opinię"
            },
            RestartVerificationUrl = _businessVerificationUrl,
            // FeedbackUrl = $"{_websiteUrl}",
            BusinessName = businessName,
            HoursUntilRemoval = 24,
            RemovedAt = removedAt
        };
    }
}