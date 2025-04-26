namespace RateRelay.Domain.Exceptions;

public class AppOkException : Exception
{
    public AppOkException(string? message) : base(message)
    {
    }

    public AppOkException(string? message, Exception innerException) : base(message, innerException)
    {
    }
}