using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Enums;

namespace RateRelay.Domain.Extensions.Account;

public static class AccountPermissionExtensions
{
    public static bool HasPermission(this AccountEntity account, Permission permission)
    {
        return (account.Permissions & (ulong)permission) == (ulong)permission;
    }
    
    public static bool HasPermission(this ulong permissions, Permission permission)
    {
        return (permissions & (ulong)permission) == (ulong)permission;
    }

    public static bool HasAnyPermission(this AccountEntity account, params Permission[] permissions)
    {
        return permissions.Any(account.HasPermission);
    }

    public static void GrantPermission(this AccountEntity account, Permission permission)
    {
        account.Permissions |= (ulong)permission;
    }

    public static void RevokePermission(this AccountEntity account, Permission permission)
    {
        account.Permissions &= ~(ulong)permission;
    }

    public static IEnumerable<Permission> GetGrantedPermissions(this AccountEntity account)
    {
        return Enum.GetValues<Permission>()
            .Where(permission => permission != Permission.None && account.HasPermission(permission));
    }

    public static string GetDisplayName(this Permission permission)
    {
        var displayAttribute = typeof(Permission).GetField(permission.ToString())
            ?.GetCustomAttribute<DisplayAttribute>();

        return displayAttribute?.Name ?? permission.ToString();
    }

    public static string GetDescription(this Permission permission)
    {
        var descriptionAttribute = typeof(Permission).GetField(permission.ToString())
            ?.GetCustomAttribute<DescriptionAttribute>();

        return descriptionAttribute?.Description ?? string.Empty;
    }
}