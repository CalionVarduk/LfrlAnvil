﻿using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace LfrlAnvil.Computable.Expressions.Constructs.Decimal;

/// <summary>
/// Represents a <see cref="Decimal"/> binary add operator construct.
/// </summary>
public sealed class ParsedExpressionAddDecimalOperator : ParsedExpressionBinaryOperator<decimal>
{
    /// <inheritdoc />
    [Pure]
    protected override Expression? TryCreateFromTwoConstants(ConstantExpression left, ConstantExpression right)
    {
        return TryGetArgumentValue( left, out var leftValue ) && TryGetArgumentValue( right, out var rightValue )
            ? Expression.Constant( leftValue + rightValue )
            : null;
    }

    /// <inheritdoc />
    [Pure]
    protected override Expression? TryCreateFromOneConstant(ConstantExpression left, Expression right)
    {
        return TryGetArgumentValue( left, out var leftValue ) && leftValue == 0
            ? right
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
        return Expression.Add( left, right );
    }
}
