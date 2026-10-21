using System.Linq.Expressions;

namespace MediatorAuthService.Application.Common.Extensions;

public static class PredicateBuilder
{
    public static Expression<Func<T, bool>> True<T>() => x => true;

    public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
    {
        ParameterExpression param = left.Parameters[0];

        // Rewrites the right-hand body onto the left-hand parameter so providers receive a single flat lambda.
        Expression rightBody = new ParameterReplacer(right.Parameters[0], param).Visit(right.Body);

        return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(left.Body, rightBody), param);
    }

    private sealed class ParameterReplacer(ParameterExpression source, ParameterExpression target) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node) => node == source ? target : base.VisitParameter(node);
    }
}
