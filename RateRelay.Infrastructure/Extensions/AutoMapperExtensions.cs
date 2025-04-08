using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace RateRelay.Infrastructure.Extensions;

public static class AutoMapperExtensions
{
    public static void AddAutoMapperConfiguration(this IServiceCollection services)
    {
        var applicationAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "RateRelay.Application");

        if (applicationAssembly == null)
        {
            applicationAssembly = Assembly.Load("RateRelay.Application");
        }

        services.AddAutoMapper(applicationAssembly);
    }
}