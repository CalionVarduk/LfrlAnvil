using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace LfrlAnvil.Computable.Expressions.Constructs.Int32;

/// <summary>
/// Represents a <see cref="Int32"/> binary multiply operator construct.
/// </summary>
public sealed class ParsedExpressionMultiplyInt32Operator : ParsedExpressionBinaryOperator<int>
{
    /// <inheritdoc />
    [Pure]
    protected override Expression? TryCreateFromTwoConstants(ConstantExpression left, ConstantExpression right)
    {
        return TryGetArgumentValue( left, out var leftValue ) && TryGetArgumentValue( right, out var rightValue )
            ? Expression.Constant( leftValue * rightValue )
            : null;
    }

    /// <inheritdoc />
    [Pure]
    protected override Expression? TryCreateFromOneConstant(ConstantExpression left, Expression right)
    {
        return TryCreateFromOneConstantInternal( right, left );
    }

    /// <inheritdoc />
    [Pure]
    protected override Expression? TryCreateFromOneConstant(Expression left, ConstantExpression right)
    {
        return TryCreateFromOneConstantInternal( left, right );
    }

    /// <inheritdoc />
    [Pure]
    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        return Expression.Multiply( left, right );
    }

    [Pure]
    private static Expression? TryCreateFromOneConstantInternal(Expression expression, ConstantExpression constant)
    {
        if ( ! TryGetArgumentValue( constant, out var value ) )
            return null;

        if ( value == 0 )
            return constant;

        if ( value == 1 )
            return expression;

        if ( value == -1 )
            return Expression.Negate( expression );

        return null;
    }
}
