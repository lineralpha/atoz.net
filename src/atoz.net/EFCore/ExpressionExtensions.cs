using System.Linq.Expressions;

namespace Atoz.EFCore;

public static class ExpressionExtensions
{
    /// <summary>
    /// Creates a combined expression <see cref="AndAlso{T}(Expression{Func{T, bool}}, Expression{Func{T, bool}})"/>
    /// that evaluates to <see langword="true"/> if and only if both current and second expressions are evaluated
    /// to <see langword="true"/>.
    /// <para />
    /// The second expression is evaluated only if the current expression is evaluated to <see langword="true"/>.
    /// </summary>
    public static Expression<Func<T, bool>> AndAlso<T>(
        this Expression<Func<T, bool>> exp,
        Expression<Func<T, bool>> newExp)
    {
        // get the parameter visitor
        var visitor = new ParameterUpdateVisitor(newExp.Parameters.First(), exp.Parameters.First());

        // replace the parameter in the new expression so both new and old expressions have the same paramter
        newExp = (visitor.Visit(newExp)! as Expression<Func<T, bool>>)!;

        // andalso => second expression is evaluated only if first one is evaluated to true.
        var binExp = Expression.AndAlso(exp.Body, newExp.Body);

        // return a new lambda
        return Expression.Lambda<Func<T, bool>>(binExp, exp.Parameters);
    }

    /// <summary>
    /// Creates a combined expression <see cref="OrElse{T}(Expression{Func{T, bool}}, Expression{Func{T, bool}})"/>
    /// that evaluates to <see langword="true"/> if either one of the expressions is evaluated to <see langword="true"/>.
    /// <para />
    /// The second expression is evaluated only if the current expression is evaluated to <see langword="false"/>.
    /// </summary>
    public static Expression<Func<T, bool>> OrElse<T>(
        this Expression<Func<T, bool>> exp,
        Expression<Func<T, bool>> newExp)
    {
        // Similar to AndAlso, just replace Expression.AndAlso with Expression.OrElse

        var visitor = new ParameterUpdateVisitor(newExp.Parameters.First(), exp.Parameters.First());
        newExp = (visitor.Visit(newExp)! as Expression<Func<T, bool>>)!;

        var binExp = Expression.OrElse(exp.Body, newExp.Body);
        return Expression.Lambda<Func<T, bool>>(binExp, exp.Parameters);
    }

    /// <summary>
    /// Creates a new lambda expression that negates the current expression.
    /// </summary>
    public static Expression<Func<T, bool>> Not<T>(this Expression<Func<T, bool>> exp)
    {
        return Expression.Lambda<Func<T, bool>>(Expression.Not(exp.Body), exp.Parameters);
    }

    private class ParameterUpdateVisitor : ExpressionVisitor
    {
        private ParameterExpression _oldParameter;
        private ParameterExpression _newParameter;

        public ParameterUpdateVisitor(ParameterExpression oldParameter, ParameterExpression newParameter)
        {
            _oldParameter = oldParameter;
            _newParameter = newParameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (ReferenceEquals(node, _oldParameter))
            {
                return _newParameter;
            }

            return base.VisitParameter(node);
        }
    }
}
