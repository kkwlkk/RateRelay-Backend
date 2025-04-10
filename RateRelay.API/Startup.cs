using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using RateRelay.API.Extensions;
using RateRelay.API.Middleware;
using RateRelay.Domain.Common;
using RateRelay.Infrastructure.Logging;
using Serilog;

namespace RateRelay.API;

public class Startup(IConfiguration configuration, IWebHostEnvironment environment)
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers(options =>
            {
                options.EnableEndpointRouting = false;
                options.ModelValidatorProviders.Clear();
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressMapClientErrors = true;
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(e => e.Value?.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? []
                        );

                    return new BadRequestObjectResult(
                        ApiResponse<object>.ValidationErrorResponse(
                            errors.Select(e => new ValidationError
                            {
                                Property = e.Key,
                                Message = string.Join(", ", e.Value),
                                Code = "INVALID_INPUT"
                            }),
                            StatusCodes.Status400BadRequest
                        )
                    );
                };
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

        services.AddRouting(options => options.LowercaseUrls = true);
        services.AddEndpointsApiExplorer();
        services.AddConfiguredSwagger();
        services.AddLogging(configuration);
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment() || env.IsStaging())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseSerilogRequestLogging(options =>
        {
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestPath", httpContext.Request.Path);
                diagnosticContext.Set("RequestMethod", httpContext.Request.Method);
            };
        });
        app.UseExceptionHandling();
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}