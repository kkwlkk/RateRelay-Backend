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
        var response = ApiResponse<T>.Create(true, data, statusCode: statusCode);
        return HandleApiResponse(response);
    }

    protected IActionResult Success(int statusCode = 204)
    {
        var response = ApiResponse<object>.Create(true, null, statusCode: statusCode);
        return HandleApiResponse(response);
    }

    protected IActionResult Success<T>(T data, Dictionary<string, object> metadata, int statusCode = 200)
    {
        var response = ApiResponse<T>.Create(true, data, metadata: metadata, statusCode: statusCode);
        return HandleApiResponse(response);
    }

    protected IActionResult Error<T>(string message, string code = null, int statusCode = 400)
    {
        var response = ApiResponse<T>.Create(false, errorMessage: message, errorCode: code, statusCode: statusCode);
        return HandleApiResponse(response);
    }

    protected IActionResult Error<T>(string message, string code = null, Dictionary<string, object> metadata = null,
        int statusCode = 400)
    {
        var response = ApiResponse<T>.Create(false, errorMessage: message, errorCode: code, metadata: metadata,
            statusCode: statusCode);
        return HandleApiResponse(response);
    }

    protected IActionResult ValidationError<T>(IEnumerable<ValidationError> errors, int statusCode = 400)
    {
        var response = ApiResponse<T>.Create(false, validationErrors: errors, statusCode: statusCode);
        return HandleApiResponse(response);
    }

    protected IActionResult ValidationError<T>(IEnumerable<ValidationError> errors, Dictionary<string, object> metadata,
        int statusCode = 400)
    {
        var response =
            ApiResponse<T>.Create(false, validationErrors: errors, metadata: metadata, statusCode: statusCode);
        return HandleApiResponse(response);
    }

    protected IActionResult PagedSuccess<T>(PagedApiResponse<T> response)
    {
        return HandleApiResponse(response);
    }
}