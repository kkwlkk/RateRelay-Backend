namespace RateRelay.Domain.Interfaces;

public interface ICurrentUserDataResolver
{
    long GetAccountId();
    string GetUsername();
}