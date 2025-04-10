using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Enums;

namespace RateRelay.Domain.Extensions;

public static class PermissionExtensions
{
    public static bool HasPermission(this ulong permissions, Permission permission)
    {
        if (permission == Permission.None)
            return true;

        return (permissions & (ulong)permission) != 0;
    }

    public static bool HasPermission(this AccountEntity account, Permission permission)
    {
        if (permission == Permission.None)
            return true;

        if ((account.Permissions & (ulong)permission) != 0)
            return true;

        if (account.Role == null) return false;

        return (account.Role.Permissions & (ulong)permission) != 0;
    }

    public static ulong AddPermission(this ulong permissions, Permission permission)
    {
        if (permission == Permission.None)
            return permissions;
            
        return permissions | (ulong)permission;
    }

    public static ulong RemovePermission(this ulong permissions, Permission permission)
    {
        if (permission == Permission.None)
            return permissions;
            
        return permissions & ~(ulong)permission;
    }

    public static ulong SetPermission(this ulong permissions, Permission permission, bool enabled)
    {
        if (enabled)
            return permissions.AddPermission(permission);
        else
            return permissions.RemovePermission(permission);
    }

    public static IEnumerable<Permission> GetIndividualPermissions(this ulong permissions)
    {
        foreach (Permission permission in Enum.GetValues(typeof(Permission)))
        {
            if (permission == Permission.None)
                continue;
                
            if ((permissions & (ulong)permission) != 0)
                yield return permission;
        }
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