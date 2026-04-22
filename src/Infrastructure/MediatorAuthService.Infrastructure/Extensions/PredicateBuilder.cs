using System.Linq.Expressions;

namespace MediatorAuthService.Infrastructure.Extensions;

public static class PredicateBuilder
{
    public static Expression<Func<T, bool>> True<T>() => x => true;

    public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
    {
        ParameterExpression param = left.Parameters[0];

        Expression body = Expression.AndAlso(left.Body, Expression.Invoke(right, param));

        return Expression.Lambda<Func<T, bool>>(body, param);
    }
}