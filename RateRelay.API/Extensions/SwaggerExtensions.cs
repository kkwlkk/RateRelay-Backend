using Microsoft.OpenApi.Models;
using RateRelay.API.Filters.Swagger;

namespace RateRelay.API.Extensions;

public static class SwaggerExtensions
{
    public static void AddConfiguredSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.EnableAnnotations();

            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "RateRelay API",
                Version = "v1",
                Description = "RateRelay API"
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description =
                    "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            options.OperationFilter<SecurityRequirementsOperationFilter>();
        });
    }
}