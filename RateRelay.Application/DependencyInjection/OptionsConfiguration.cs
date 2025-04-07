using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RateRelay.Application.DependencyInjection;

public static class OptionsConfiguration
{
    public static void ConfigureOptions(this IServiceCollection services, IConfiguration configuration)
    {
        try
        {
            const string configAssemblyName = "RateRelay.Infrastructure";

            var configAssembly = AppDomain.CurrentDomain.GetAssemblies()
                                     .FirstOrDefault(a => a.GetName().Name == configAssemblyName)
                                 ?? Assembly.Load(configAssemblyName);

            const string configNamespace = "RateRelay.Infrastructure.Configuration";

            var optionsTypes = configAssembly
                .GetTypes()
                .Where(type =>
                    type.Name.EndsWith("Options") &&
                    type.Namespace is configNamespace &&
                    type is { IsClass: true, IsAbstract: false });

            foreach (var optionsType in optionsTypes)
            {
                var sectionNameField = optionsType.GetField("SectionName", BindingFlags.Public | BindingFlags.Static);
                var sectionNameProperty =
                    optionsType.GetProperty("SectionName", BindingFlags.Public | BindingFlags.Static);

                var sectionName = sectionNameField?.GetValue(null) as string ??
                                  sectionNameProperty?.GetValue(null) as string;

                if (string.IsNullOrEmpty(sectionName))
                {
                    Console.WriteLine($"Skipping {optionsType.Name}: Missing or invalid SectionName field/property.");
                    continue;
                }

                var addOptionsMethod = typeof(OptionsConfiguration)
                    .GetMethod(nameof(AddOptions),
                        BindingFlags.NonPublic | BindingFlags.Static)?
                    .MakeGenericMethod(optionsType);

                addOptionsMethod?.Invoke(null, [services, configuration, sectionName]);
            }
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine("An error occurred while configuring options.");
            Console.Error.WriteLine(exception.Message);
        }
    }

    private static void AddOptions<TOptions>(IServiceCollection services, IConfiguration configuration,
        string sectionName)
        where TOptions : class
    {
        try
        {
            services.AddOptions<TOptions>()
                .Bind(configuration.GetRequiredSection(sectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to register options: {typeof(TOptions).Name} for section: {sectionName}");
            Console.Error.WriteLine(ex.Message);
        }
    }
}