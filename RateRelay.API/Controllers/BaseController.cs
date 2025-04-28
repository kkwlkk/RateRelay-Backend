using Microsoft.AspNetCore.Mvc;
using RateRelay.Domain.Common;

namespace RateRelay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    private IActionResult HandleApiResponse<T>(ApiResponse<T> response)
    {
        return StatusCode(response.StatusCode, response);
    }

    protected IActionResult Success<T>(T data, int statusCode = 200)
    {
        var response = ApiResponse<T>.SuccessResponse(data, statusCode);
        return HandleApiResponse(response);
    }

    protected IActionResult Success<T>(T data, Dictionary<string, object> metadata, int statusCode = 200)
    {
        var response = ApiResponse<T>.SuccessResponse(data, metadata, statusCode);
        return HandleApiResponse(response);
    }

    protected IActionResult Error<T>(string message, string code = null, int statusCode = 400)
    {
        var response = ApiResponse<T>.ErrorResponse(message, code, statusCode);
        return HandleApiResponse(response);
    }

    protected IActionResult Error<T>(string message, string code = null, Dictionary<string, object> metadata = null, int statusCode = 400)
    {
        var response = ApiResponse<T>.ErrorResponse(message, code, metadata, statusCode);
        return HandleApiResponse(response);
    }

    protected IActionResult ValidationError<T>(IEnumerable<ValidationError> errors, int statusCode = 400)
    {
        var response = ApiResponse<T>.ValidationErrorResponse(errors, statusCode);
        return HandleApiResponse(response);
    }

    protected IActionResult ValidationError<T>(IEnumerable<ValidationError> errors, Dictionary<string, object> metadata, int statusCode = 400)
    {
        var response = ApiResponse<T>.ValidationErrorResponse(errors, metadata, statusCode);
        return HandleApiResponse(response);
    }

    protected IActionResult WithMetadata<T>(ApiResponse<T> response, Dictionary<string, object> metadata)
    {
        response.WithMetadata(metadata);
        return HandleApiResponse(response);
    }

    protected IActionResult WithMetadataValue<T>(ApiResponse<T> response, string key, object value)
    {
        response.WithMetadataValue(key, value);
        return HandleApiResponse(response);
    }
}