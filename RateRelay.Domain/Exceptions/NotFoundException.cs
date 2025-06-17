namespace RateRelay.Domain.Exceptions;

public class NotFoundException : Exception
{
    public string? ErrorCode { get; }
    public Dictionary<string, object> Metadata { get; }

    public NotFoundException(string message) : base(message)
    {
        Metadata = new Dictionary<string, object>();
    }

    public NotFoundException(string message, string? errorCode) : base(message)
    {
        ErrorCode = errorCode;
        Metadata = new Dictionary<string, object>();
    }

    public NotFoundException(string message, Dictionary<string, object>? metadata) : base(message)
    {
        Metadata = metadata ?? new Dictionary<string, object>();
    }

    public NotFoundException(string message, string? errorCode, Dictionary<string, object>? metadata) : base(message)
    {
        ErrorCode = errorCode;
        Metadata = metadata ?? new Dictionary<string, object>();
    }

    public NotFoundException(string message, Exception innerException) : base(message, innerException)
    {
        Metadata = new Dictionary<string, object>();
    }

    public NotFoundException(string message, string? errorCode, Exception innerException) : base(message, innerException)
    {
        ErrorCode = errorCode;
        Metadata = new Dictionary<string, object>();
    }

    public NotFoundException(string message, string? errorCode, Dictionary<string, object>? metadata, Exception innerException) 
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        Metadata = metadata ?? new Dictionary<string, object>();
    }
}