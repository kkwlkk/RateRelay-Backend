using System.Text.Json;
using RateRelay.Application.Helpers;
using RateRelay.Domain.Common;
using ValidationException = RateRelay.Application.Exceptions.ValidationException;

namespace RateRelay.API.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            if (!ExceptionHelper.ShouldSkipLogging(ex))
                logger.LogError(ex, "An unhandled exception occurred");

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = GetStatusCode(exception);
        var response = CreateResponse(exception, statusCode);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private static int GetStatusCode(Exception exception) => exception switch
    {
        ValidationException => StatusCodes.Status400BadRequest,
        FluentValidation.ValidationException => StatusCodes.Status400BadRequest,
        KeyNotFoundException => StatusCodes.Status404NotFound,
        UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
        _ => StatusCodes.Status500InternalServerError
    };

    private static object CreateResponse(Exception exception, int statusCode)
    {
        switch (exception)
        {
            case ValidationException validationException:
                return ApiResponse<object>.ValidationErrorResponse(validationException.ValidationErrors, statusCode);
            case FluentValidation.ValidationException fluentValidationEx:
            {
                var validationErrors = fluentValidationEx.Errors.Select(e => new ValidationError
                {
                    Property = e.PropertyName,
                    Message = e.ErrorMessage,
                    Code = e.ErrorCode,
                    AttemptedValue = e.AttemptedValue
                });
                return ApiResponse<object>.ValidationErrorResponse(validationErrors, statusCode);
            }
            default:
                return ApiResponse<object>.ErrorResponse(exception.Message, null, statusCode);
        }
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}