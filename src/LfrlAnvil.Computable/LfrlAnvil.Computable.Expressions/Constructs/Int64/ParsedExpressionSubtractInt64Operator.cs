﻿using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace LfrlAnvil.Computable.Expressions.Constructs.Int64;

/// <summary>
/// Represents a <see cref="Int64"/> binary subtract operator construct.
/// </summary>
public sealed class ParsedExpressionSubtractInt64Operator : ParsedExpressionBinaryOperator<long>
{
    /// <inheritdoc />
    [Pure]
    protected override Expression? TryCreateFromTwoConstants(ConstantExpression left, ConstantExpression right)
    {
        return TryGetArgumentValue( left, out var leftValue ) && TryGetArgumentValue( right, out var rightValue )
            ? Expression.Constant( leftValue - rightValue )
            : null;
    }

    /// <inheritdoc />
    [Pure]
    protected override Expression? TryCreateFromOneConstant(ConstantExpression left, Expression right)
    {
        return TryGetArgumentValue( left, out var leftValue ) && leftValue == 0
            ? Expression.Negate( right )
            : null;
    }

    /// <inheritdoc />
    [Pure]
    protected override Expression? TryCreateFromOneConstant(Expression left, ConstantExpression right)
    {
        return TryGetArgumentValue( right, out var rightValue ) && rightValue == 0
            ? left
            : null;
    }

    /// <inheritdoc />
    [Pure]
    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        return Expression.Subtract( left, right );
    }
}
