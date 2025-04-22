using RateRelay.Domain.Interfaces;
using RateRelay.Domain.Interfaces.Services;

namespace RateRelay.Infrastructure.Services;

public class CurrentUserContext(ICurrentUserDataResolver currentUserDataResolver)
{
    public bool IsAuthenticated => currentUserDataResolver.IsAuthenticated();
    
    public long AccountId => currentUserDataResolver.GetAccountId();
    
    public string Username => currentUserDataResolver.GetUsername();
    
    public string Email => currentUserDataResolver.GetEmail();
    
    public ulong Permissions => currentUserDataResolver.GetPermissions();
    
    public bool HasPermission(Domain.Enums.Permission permission)
    {
        return IsAuthenticated && Domain.Extensions.PermissionExtensions.HasPermission(Permissions, permission);
    }
}