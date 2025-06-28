namespace RateRelay.Domain.Exceptions;

public class UnauthorizedException : AppException
{
    private const string DefaultErrorCode = "UNAUTHORIZED";
    private const string DefaultMessage = "Access denied. You are not authorized to perform this action.";

    public UnauthorizedException() : base(DefaultMessage, DefaultErrorCode)
    {
    }

    public UnauthorizedException(string? message) : base(message, DefaultErrorCode)
    {
    }

    public UnauthorizedException(string? message, Dictionary<string, object>? metadata)
        : base(message, DefaultErrorCode, metadata)
    {
    }

    public UnauthorizedException(string? message, Exception innerException)
        : base(message, DefaultErrorCode, innerException)
    {
    }

    public UnauthorizedException(string? message, Dictionary<string, object>? metadata, Exception innerException)
        : base(message, DefaultErrorCode, metadata, innerException)
    {
    }
}