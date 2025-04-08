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

    protected IActionResult Error<T>(string message, string code = null, int statusCode = 400)
    {
        var response = ApiResponse<T>.ErrorResponse(message, code, statusCode);
        return HandleApiResponse(response);
    }

    protected IActionResult ValidationError<T>(IEnumerable<ValidationError> errors, int statusCode = 400)
    {
        var response = ApiResponse<T>.ValidationErrorResponse(errors, statusCode);
        return HandleApiResponse(response);
    }

    protected IActionResult WithMetadata<T>(ApiResponse<T> response, Action<MetadataResponse> configureMetadata)
    {
        response.WithMetadata(configureMetadata);
        return HandleApiResponse(response);
    }
}