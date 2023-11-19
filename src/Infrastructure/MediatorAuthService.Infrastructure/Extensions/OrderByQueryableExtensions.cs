using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace MediatorAuthService.Infrastructure.Extensions;

internal static class OrderByQueryableExtensions
{
    internal static IQueryable<T> OrderBy<T>(this IQueryable<T> source, string orderKey, string orderType)
    {
        bool isValidProperty = IsValidProperty(typeof(T), orderKey);

        if (!isValidProperty)
            throw new ValidationException("The entered property name is invalid!");

        Expression expression = source.Expression;

        ParameterExpression parameter = Expression.Parameter(typeof(T), "x");

        MemberExpression selector = Expression.PropertyOrField(parameter, orderKey);

        string method = string.Equals(orderType, "descending", StringComparison.OrdinalIgnoreCase)
             ? "OrderByDescending" : "OrderBy";

        expression = Expression.Call(
            typeof(Queryable), 
            method,
            [source.ElementType, selector.Type],
            expression,
            Expression.Quote(Expression.Lambda(selector, parameter)));

        return source.Provider.CreateQuery<T>(expression);
    }

    private static bool IsValidProperty(Type type, string findPropertyName)
    {
        return type.GetProperties().Any(x => x.Name.Equals(findPropertyName, StringComparison.Ordinal));
    }
}