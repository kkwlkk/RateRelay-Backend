namespace RateRelay.Domain.Exceptions;

public class DomainException : Exception
{
    public string? ErrorCode { get; }
    public Dictionary<string, object> Metadata { get; }
    public int? StatusCode { get; }
    private const string DefaultMessage = "An application error occurred.";

    public DomainException(string? message = null) : base(message ?? DefaultMessage)
    {
        Metadata = new Dictionary<string, object>();
    }

    public DomainException(string? message, string? errorCode) : base(message ?? DefaultMessage)
    {
        ErrorCode = errorCode;
        Metadata = new Dictionary<string, object>();
    }

    public DomainException(string? message, int statusCode) : base(message ?? DefaultMessage)
    {
        StatusCode = statusCode;
        Metadata = new Dictionary<string, object>();
    }

    public DomainException(string? message, string? errorCode, int statusCode) : base(message ?? DefaultMessage)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
        Metadata = new Dictionary<string, object>();
    }

    public DomainException(string? message, Dictionary<string, object>? metadata) : base(message ?? DefaultMessage)
    {
        Metadata = metadata ?? new Dictionary<string, object>();
    }

    public DomainException(string? message, string? errorCode, Dictionary<string, object>? metadata) : base(message ?? DefaultMessage)
    {
        ErrorCode = errorCode;
        Metadata = metadata ?? new Dictionary<string, object>();
    }

    public DomainException(string? message, string? errorCode, int statusCode, Dictionary<string, object>? metadata) : base(message ?? DefaultMessage)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
        Metadata = metadata ?? new Dictionary<string, object>();
    }

    public DomainException(string? message, Exception innerException) : base(message ?? DefaultMessage, innerException)
    {
        Metadata = new Dictionary<string, object>();
    }

    public DomainException(string? message, string? errorCode, Exception innerException) : base(message ?? DefaultMessage, innerException)
    {
        ErrorCode = errorCode;
        Metadata = new Dictionary<string, object>();
    }

    public DomainException(string? message, string? errorCode, int statusCode, Exception innerException) : base(message ?? DefaultMessage, innerException)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
        Metadata = new Dictionary<string, object>();
    }

    public DomainException(string? message, string? errorCode, int statusCode, Dictionary<string, object>? metadata, Exception innerException) 
        : base(message ?? DefaultMessage, innerException)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
        Metadata = metadata ?? new Dictionary<string, object>();
    }
}