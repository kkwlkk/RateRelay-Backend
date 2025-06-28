using RateRelay.Domain.Extensions.Account;
using RateRelay.Domain.Interfaces;

namespace RateRelay.Infrastructure.Services;

public class CurrentUserContext(ICurrentUserDataResolver currentUserDataResolver)
{
    public bool IsAuthenticated => currentUserDataResolver.IsAuthenticated();
    
    public long AccountId => currentUserDataResolver.GetAccountId();
    
    public string Username => currentUserDataResolver.GetUsername();
    
    public string Email => currentUserDataResolver.GetEmail();

    private ulong Permissions => currentUserDataResolver.GetPermissions();
    
    public bool HasPermission(Domain.Enums.Permission permission)
    {
        return IsAuthenticated && Permissions.HasPermission(permission);
    }
}