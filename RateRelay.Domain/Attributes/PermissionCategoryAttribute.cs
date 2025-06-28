using RateRelay.Domain.Enums;

namespace RateRelay.Domain.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class PermissionCategoryAttribute(PermissionCategory category) : Attribute
{
    public PermissionCategory Category { get; } = category;
}