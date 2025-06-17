using System.Text.Json;
using RateRelay.Application.Exceptions;
using RateRelay.Application.Helpers;
using RateRelay.Domain.Exceptions;
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
        NotFoundException => StatusCodes.Status404NotFound,
        AppException => StatusCodes.Status400BadRequest,
        InvalidOperationException => StatusCodes.Status400BadRequest,
        AppOkException => StatusCodes.Status200OK,
        ForbiddenException => StatusCodes.Status403Forbidden,
        _ => StatusCodes.Status500InternalServerError
    };

    private static object CreateResponse(Exception exception, int statusCode)
    {
        switch (exception)
        {
            case ValidationException validationException:
                return ApiResponse<object>.Create(
                    false,
                    validationErrors: validationException.ValidationErrors,
                    statusCode: statusCode);

            case FluentValidation.ValidationException fluentValidationEx:
            {
                var validationErrors = fluentValidationEx.Errors.Select(e => new ValidationError
                {
                    Property = e.PropertyName,
                    Message = e.ErrorMessage,
                    Code = e.ErrorCode,
                    AttemptedValue = e.AttemptedValue
                });
                return ApiResponse<object>.Create(
                    false,
                    validationErrors: validationErrors,
                    statusCode: statusCode);
            }

            case AppException appException:
                return ApiResponse<object>.Create(
                    false,
                    errorMessage: appException.Message,
                    errorCode: appException.ErrorCode,
                    metadata: appException.Metadata,
                    statusCode: statusCode);
            
            case NotFoundException notFoundException:
                return ApiResponse<object>.Create(
                    false,
                    errorMessage: notFoundException.Message,
                    errorCode: notFoundException.ErrorCode,
                    statusCode: statusCode);

            default:
                return ApiResponse<object>.Create(
                    false,
                    errorMessage: "An unexpected error occurred. Please try again later.",
                    statusCode: statusCode);
        }
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static void UseExceptionHandling(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}