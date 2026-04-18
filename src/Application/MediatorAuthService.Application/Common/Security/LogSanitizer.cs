using System.Reflection;

namespace MediatorAuthService.Application.Common.Security;

public static class LogSanitizer
{
    public static object? Sanitize(object? obj)
    {
        if (obj is null)
            return null;

        Type? type = obj.GetType();

        if (type.IsPrimitive || obj is string)
            return obj;

        Dictionary<string, object?> result = [];

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var value = prop.GetValue(obj);

            bool isSensitive = Attribute.IsDefined(prop, typeof(SensitiveDataAttribute));

            result[prop.Name] = isSensitive
                ? "[REDACTED]"
                : Sanitize(value);
        }

        return result;
    }
}