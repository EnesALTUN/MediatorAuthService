using MediatorAuthService.Domain.Core.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace MediatorAuthService.Infrastructure.Extensions;

internal static class OrderByQueryableExtensions
{
    internal static IQueryable<T> OrderBy<T>(this IQueryable<T> source, string orderKey, string orderType)
    {
        PropertyInfo? property = typeof(T)
            .GetProperties()
            .SingleOrDefault(x =>
                x.Name.Equals(orderKey, StringComparison.Ordinal) &&
                !x.IsDefined(typeof(NotSortableAttribute), inherit: true))
            ?? throw new ValidationException("The entered property name is invalid or not allowed for sorting.");

        Expression expression = source.Expression;

        ParameterExpression parameter = Expression.Parameter(typeof(T), "x");

        MemberExpression selector = Expression.Property(parameter, property);

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
}