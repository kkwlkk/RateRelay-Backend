using Google.Apis.Auth;
using RateRelay.Application.Exceptions;
using ValidationException = FluentValidation.ValidationException;

namespace RateRelay.Application.Helpers;

public static class ExceptionHelper
{
    public static bool ShouldSkipLogging(Exception ex)
    {
        return ex is UnauthorizedAccessException
            or KeyNotFoundException
            or ValidationException
            or InvalidJwtException
            or NotFoundException 
            or AppException;
    }
}