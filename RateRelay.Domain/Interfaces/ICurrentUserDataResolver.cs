namespace RateRelay.Domain.Interfaces;

public interface ICurrentUserDataResolver
{
    long GetAccountId();
    string GetUsername();
    bool IsAuthenticated();
    string GetEmail();
    ulong GetPermissions();
    bool TryGetAccountId(out long accountId);
    T GetClaimValue<T>(string claimType, T defaultValue = default);
}