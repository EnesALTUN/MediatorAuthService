using System.Linq.Expressions;

namespace MediatorAuthService.Infrastructure.Extensions;

internal static class OrderByQueryableExtensions
{
    internal static IQueryable<T> OrderBy<T>(this IQueryable<T> source, string orderKey, string orderType)
    {
        bool isValidProperty = IsValidProperty(typeof(T), orderKey);

        if (!isValidProperty)
            throw new Exception("The entered property name is invalid!");

        var expression = source.Expression;

        var parameter = Expression.Parameter(typeof(T), "x");

        var selector = Expression.PropertyOrField(parameter, orderKey);

        var method = string.Equals(orderType, "descending", StringComparison.OrdinalIgnoreCase)
             ? "OrderByDescending" : "OrderBy";

        expression = Expression.Call(
            typeof(Queryable), 
            method,
            new Type[] { source.ElementType, selector.Type },
            expression,
            Expression.Quote(Expression.Lambda(selector, parameter)));

        return source.Provider.CreateQuery<T>(expression);
    }

    private static bool IsValidProperty(Type type, string findPropertyName)
    {
        return type.GetProperties().Any(x => x.Name.Equals(findPropertyName));
    }
}