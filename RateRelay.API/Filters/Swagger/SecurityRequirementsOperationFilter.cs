using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using RateRelay.API.Attributes.Auth;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace RateRelay.API.Filters.Swagger;

public class SecurityRequirementsOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasRequirePermissionAttribute = context.MethodInfo.DeclaringType != null && context.MethodInfo.DeclaringType
            .GetCustomAttributes(true)
            .Union(context.MethodInfo.GetCustomAttributes(true))
            .Any(attr => attr is RequirePermissionAttribute);

        var hasAuthorizeAttribute = context.MethodInfo.DeclaringType != null && context.MethodInfo.DeclaringType
            .GetCustomAttributes(true)
            .Union(context.MethodInfo.GetCustomAttributes(true))
            .Any(attr => attr is AuthorizeAttribute);

        if (!hasRequirePermissionAttribute && !hasAuthorizeAttribute) return;

        operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });
        operation.Responses.Add("403", new OpenApiResponse { Description = "Forbidden" });

        operation.Security.Add(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new List<string>()
            }
        });
    }
}