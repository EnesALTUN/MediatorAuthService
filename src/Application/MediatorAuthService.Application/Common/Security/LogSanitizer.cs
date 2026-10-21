using System.Collections;
using System.Reflection;

namespace MediatorAuthService.Application.Common.Security;

public static class LogSanitizer
{
    public const string Redacted = "[REDACTED]";

    private const int MaxDepth = 5;
    private const int MaxCollectionItems = 20;

    public static object? Sanitize(object? obj) => Sanitize(obj, 0);

    /// <summary>
    /// Keeps the first character and the domain of an e-mail address, e.g. "j***@example.com".
    /// </summary>
    public static string? Mask(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        int atIndex = value.IndexOf('@');

        return atIndex > 0
            ? $"{value[0]}***{value[atIndex..]}"
            : $"{value[0]}***";
    }

    private static object? Sanitize(object? obj, int depth)
    {
        if (obj is null)
            return null;

        Type type = obj.GetType();

        if (IsScalar(type))
            return obj;

        if (depth >= MaxDepth)
            return type.Name;

        if (obj is IEnumerable enumerable)
            return enumerable.Cast<object?>().Take(MaxCollectionItems).Select(item => Sanitize(item, depth + 1)).ToList();

        Dictionary<string, object?> result = [];

        foreach (PropertyInfo prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.CanRead || prop.GetIndexParameters().Length != 0)
                continue;

            object? value = prop.GetValue(obj);

            result[prop.Name] = prop.IsDefined(typeof(SensitiveDataAttribute))
                ? Redacted
                : prop.IsDefined(typeof(PersonalDataAttribute))
                    ? Mask(value?.ToString())
                    : Sanitize(value, depth + 1);
        }

        return result;
    }

    private static bool IsScalar(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;

        return type.IsPrimitive
            || type.IsEnum
            || type == typeof(string)
            || type == typeof(decimal)
            || type == typeof(Guid)
            || type == typeof(DateTime)
            || type == typeof(DateTimeOffset)
            || type == typeof(DateOnly)
            || type == typeof(TimeOnly)
            || type == typeof(TimeSpan)
            || type == typeof(Uri);
    }
}
