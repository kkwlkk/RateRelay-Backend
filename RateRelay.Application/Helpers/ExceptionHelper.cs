using FluentValidation;
using Google.Apis.Auth;

namespace RateRelay.Application.Helpers;

public static class ExceptionHelper
{
    public static bool ShouldSkipLogging(Exception ex)
    {
        return ex is UnauthorizedAccessException
            or KeyNotFoundException
            or ValidationException
            or InvalidJwtException;
    }
}