using MediatorAuthService.Application.Exceptions;
using MediatorAuthService.Domain.Core.Attributes;
using System.Linq.Expressions;
using System.Reflection;

namespace MediatorAuthService.Infrastructure.Extensions;

internal static class OrderByQueryableExtensions
{
    private const string IdPropertyName = "Id";

    /// <summary>
    /// Orders by the given property name (case-insensitive). Properties marked with NotSortable are rejected.
    /// When the entity has an Id property it is used as a tie-breaker so that pages are stable.
    /// </summary>
    internal static IQueryable<T> OrderBy<T>(this IQueryable<T> source, string orderKey, string orderType)
    {
        PropertyInfo property = typeof(T)
            .GetProperties()
            .SingleOrDefault(x =>
                x.Name.Equals(orderKey, StringComparison.OrdinalIgnoreCase) &&
                !x.IsDefined(typeof(NotSortableAttribute), inherit: true))
            ?? throw new BusinessException("The entered property name is invalid or not allowed for sorting.");

        bool descending = string.Equals(orderType, "descending", StringComparison.OrdinalIgnoreCase);

        IQueryable<T> ordered = ApplyOrdering(source, property, descending ? nameof(Queryable.OrderByDescending) : nameof(Queryable.OrderBy));

        PropertyInfo? idProperty = typeof(T).GetProperty(IdPropertyName);

        return idProperty is not null && property.Name != IdPropertyName
            ? ApplyOrdering(ordered, idProperty, nameof(Queryable.ThenBy))
            : ordered;
    }

    private static IQueryable<T> ApplyOrdering<T>(IQueryable<T> source, PropertyInfo property, string method)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(T), "x");

        MemberExpression selector = Expression.Property(parameter, property);

        Expression expression = Expression.Call(
            typeof(Queryable),
            method,
            [source.ElementType, selector.Type],
            source.Expression,
            Expression.Quote(Expression.Lambda(selector, parameter)));

        return source.Provider.CreateQuery<T>(expression);
    }
}
