namespace RateRelay.Infrastructure.Interfaces;

public interface IEmailTemplateService
{
    Task<string> RenderTemplateAsync<T>(string templateName, T model);
}