using RateRelay.Infrastructure.Models.Email;
using RateRelay.Infrastructure.Models.Email.Base;

namespace RateRelay.Infrastructure.Interfaces;

public interface IEmailModelBuilderService
{
    BaseEmailModel BuildBaseModel(User user, string subject);
    WelcomeEmailModel BuildWelcomeModel(User user);
    BusinessVerificationIntroEmailModel BuildBusinessVerificationIntroModel(User user, int daysFromRegistration);
    IncompleteVerificationEmailModel BuildIncompleteVerificationModel(User user, string businessName, DateTime removedAt);
}