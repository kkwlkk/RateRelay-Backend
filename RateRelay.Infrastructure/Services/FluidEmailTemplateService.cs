using RateRelay.Infrastructure.Interfaces;
using System.Reflection;
using System.Text;
using Fluid;
using WebMarkupMin.Core;

namespace RateRelay.Infrastructure.Services;

public class FluidEmailTemplateService : IEmailTemplateService
{
    private readonly string _templatesPath;
    private readonly FluidParser _parser;
    private readonly TemplateOptions _templateOptions;

    public FluidEmailTemplateService()
    {
        var assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        if (assemblyLocation is null || !Directory.Exists(assemblyLocation))
            throw new DirectoryNotFoundException("Could not determine assembly location for email templates.");

        _templatesPath = Path.Combine(assemblyLocation, "EmailTemplates");
        _parser = new FluidParser();

        _templateOptions = new TemplateOptions
        {
            MemberAccessStrategy = new UnsafeMemberAccessStrategy()
        };
    }

    public async Task<string> RenderTemplateAsync<T>(string templateName, T model)
    {
        var templatePath = Path.Combine(_templatesPath, $"{templateName}.liquid");
        
        if (!File.Exists(templatePath))
            throw new FileNotFoundException($"Template not found: {templatePath}");

        var templateContent = await File.ReadAllTextAsync(templatePath, Encoding.UTF8);
        
        if (!_parser.TryParse(templateContent, out var template, out var error))
            throw new InvalidOperationException($"Failed to parse template: {error}");

        var context = new TemplateContext(model, _templateOptions);
        context.SetValue("current_year", DateTime.Now.Year);

        var html = await template.RenderAsync(context);

        var minifier = new HtmlMinifier(new HtmlMinificationSettings
        {
            RemoveOptionalEndTags = false,
            RemoveEmptyAttributes = false,
            WhitespaceMinificationMode = WhitespaceMinificationMode.Safe
        });

        var minifiedResult = minifier.Minify(html, generateStatistics: false);

        return minifiedResult.MinifiedContent;
    }
}