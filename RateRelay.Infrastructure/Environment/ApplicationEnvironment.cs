namespace RateRelay.Infrastructure.Environment;

public sealed class ApplicationEnvironment
{
    private const string AspNetCoreEnvironmentVariable = "ASPNETCORE_ENVIRONMENT";
    private const string DefaultEnvironment = "Production";
    private string Name { get; }

    public bool IsDevelopment => Name.Equals("Development", StringComparison.OrdinalIgnoreCase);
    public bool IsProduction => Name.Equals("Production", StringComparison.OrdinalIgnoreCase);
    public bool IsStaging => Name.Equals("Staging", StringComparison.OrdinalIgnoreCase);
    public bool IsTest => Name.Equals("Test", StringComparison.OrdinalIgnoreCase);

    private ApplicationEnvironment(string name)
    {
        Name = !string.IsNullOrWhiteSpace(name) ? name : DefaultEnvironment;
    }

    public static ApplicationEnvironment Current()
    {
        var environmentName = System.Environment.GetEnvironmentVariable(AspNetCoreEnvironmentVariable);
        return new ApplicationEnvironment(environmentName);
    }

    public static ApplicationEnvironment Create(string name)
    {
        return new ApplicationEnvironment(name);
    }

    public void EnsureEnvironmentVariableIsSet()
    {
        var current = System.Environment.GetEnvironmentVariable(AspNetCoreEnvironmentVariable);
        if (string.IsNullOrEmpty(current))
        {
            System.Environment.SetEnvironmentVariable(AspNetCoreEnvironmentVariable, Name);
        }
    }

    public string GetSettingsFileName(string baseFileName = "appsettings")
    {
        return $"{baseFileName}.{Name}.json";
    }

    public override string ToString() => Name;
}

public static class EnvironmentService
{
    public static ApplicationEnvironment ConfigureEnvironment()
    {
        var environment = ApplicationEnvironment.Current();
        environment.EnsureEnvironmentVariableIsSet();
        return environment;
    }

    public static string GetEnvironmentConfigPath(ApplicationEnvironment environment,
        string baseFileName = "appsettings")
    {
        return environment.GetSettingsFileName(baseFileName);
    }
}