using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RateRelay.Application.MediatR.Behaviors;
using RateRelay.Infrastructure.Environment;

namespace RateRelay.Application.MediatR;

public static class MediatRConfiguration
{
    public static void AddMediatR(this IServiceCollection services)
    {
        var applicationAssembly = Assembly.GetExecutingAssembly();

        services.AddValidatorsFromAssembly(applicationAssembly);
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(applicationAssembly);
            RegisterBehaviors(cfg);
        });
    }

    private static void RegisterBehaviors(MediatRServiceConfiguration configuration)
    {
        configuration.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        if (ApplicationEnvironment.Current().IsDevelopment || ApplicationEnvironment.Current().IsStaging)
        {
            configuration.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        }
    }
}