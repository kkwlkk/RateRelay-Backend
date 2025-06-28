using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace RateRelay.Domain.Extensions;

public static class PropertyInfoExtensions
{
    public static int? MaxLength(this PropertyInfo property) =>
        property.GetCustomAttribute<MaxLengthAttribute>()?.Length;

    public static string ForeignKey(this PropertyInfo property) =>
        property.GetCustomAttribute<ForeignKeyAttribute>()?.Name ?? string.Empty;

    public static bool IsNotMapped(this PropertyInfo property) =>
        property.GetCustomAttribute<NotMappedAttribute>() != null;

    public static string TableName(this Type entityType) =>
        entityType.GetCustomAttribute<TableAttribute>()?.Name ?? entityType.Name;

    public static Dictionary<string, object> GetAllAttributes(this PropertyInfo property) =>
        property.GetCustomAttributes()
            .ToDictionary<Attribute, string, object>(attr => attr.GetType().Name, attr => attr);
}

public static class TypeExtensions
{
    public static PropertyInfo? GetPropertySafe(this Type type, string propertyName) =>
        type.GetProperty(propertyName);

    public static int? GetMaxLength(this Type entityType, string propertyName) =>
        entityType.GetPropertySafe(propertyName)?.MaxLength();
    
    public static int? GetMaxLength<T>(string propertyName) =>
        typeof(T).GetMaxLength(propertyName);
    
    public static int GetMaxLengthOrDefault<T>(string propertyName, int defaultValue = 1000) =>
        GetMaxLength<T>(propertyName) ?? defaultValue;

    public static string GetForeignKey(this Type entityType, string propertyName) =>
        entityType.GetPropertySafe(propertyName)?.ForeignKey() ?? string.Empty;

    public static bool IsPropertyNotMapped(this Type entityType, string propertyName) =>
        entityType.GetPropertySafe(propertyName)?.IsNotMapped() ?? false;
}